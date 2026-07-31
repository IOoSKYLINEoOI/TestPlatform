using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestPlatform.Application.Users;

namespace TestPlatform.Infrastructure.Identity.Management;

public sealed class KeycloakIdentityAccountProvisioner(
    HttpClient httpClient,
    IOptions<KeycloakManagementOptions> options,
    ILogger<KeycloakIdentityAccountProvisioner> logger) : IIdentityAccountProvisioner
{
    private readonly KeycloakManagementOptions _options = options.Value;

    public async Task<Result<ProvisionedIdentityAccount>> CreateAsync(
        IdentityAccountProvisioningRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await GetAccessTokenAsync(cancellationToken);
            if (token is null)
            {
                return Result.Failure<ProvisionedIdentityAccount>(
                    IdentityAccountErrors.ProvisioningFailed);
            }

            if (await UsernameExistsAsync(token, request.Username, cancellationToken))
            {
                return Result.Failure<ProvisionedIdentityAccount>(
                    IdentityAccountErrors.UsernameAlreadyExists);
            }

            if (await EmployeeNumberExistsAsync(token, request.EmployeeNumber, cancellationToken))
            {
                return Result.Failure<ProvisionedIdentityAccount>(
                    IdentityAccountErrors.EmployeeNumberAlreadyExists);
            }

            var userIdResult = await CreateUserAsync(token, request, cancellationToken);
            if (userIdResult.IsFailure)
            {
                return Result.Failure<ProvisionedIdentityAccount>(userIdResult.Error);
            }

            var userId = userIdResult.Value;
            var roleAssigned = await AssignRealmRoleAsync(
                token,
                userId,
                request.Role,
                cancellationToken);

            if (!roleAssigned)
            {
                await CompensateUserCreationAsync(token, userId);
                return Result.Failure<ProvisionedIdentityAccount>(
                    IdentityAccountErrors.ProvisioningFailed);
            }

            logger.LogInformation(
                "Created Keycloak user {KeycloakUserId} with role {Role}.",
                userId,
                request.Role);

            return Result.Success(new ProvisionedIdentityAccount(
                userId,
                request.Username,
                request.EmployeeNumber,
                request.Role));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to provision a Keycloak user.");
            return Result.Failure<ProvisionedIdentityAccount>(
                IdentityAccountErrors.ProvisioningFailed);
        }
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
        });
        using var response = await httpClient.PostAsync(
            $"realms/{Uri.EscapeDataString(_options.Realm)}/protocol/openid-connect/token",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "Keycloak service-account token request failed with status {StatusCode}.",
                response.StatusCode);
            return null;
        }

        var token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(
            cancellationToken);
        return token?.AccessToken;
    }

    private async Task<bool> UsernameExistsAsync(
        string token,
        string username,
        CancellationToken cancellationToken)
    {
        var users = await GetUsersAsync(
            token,
            $"username={Uri.EscapeDataString(username)}&exact=true",
            cancellationToken);
        return users.Any(user => string.Equals(
            user.Username,
            username,
            StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> EmployeeNumberExistsAsync(
        string token,
        string employeeNumber,
        CancellationToken cancellationToken)
    {
        var query = Uri.EscapeDataString($"employee_number:{employeeNumber}");
        var users = await GetUsersAsync(token, $"q={query}", cancellationToken);
        return users.Any(user =>
            user.Attributes?.TryGetValue("employee_number", out var values) == true
            && values.Contains(employeeNumber, StringComparer.Ordinal));
    }

    private async Task<IReadOnlyList<KeycloakUserResponse>> GetUsersAsync(
        string token,
        string query,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/users?{query}",
            token);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<KeycloakUserResponse>>(
            cancellationToken) ?? [];
    }

    private async Task<Result<string>> CreateUserAsync(
        string token,
        IdentityAccountProvisioningRequest account,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/users",
            token);
        request.Content = JsonContent.Create(new
        {
            username = account.Username,
            enabled = true,
            attributes = new Dictionary<string, string[]>
            {
                ["employee_number"] = [account.EmployeeNumber],
            },
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = account.TemporaryPassword,
                    temporary = true,
                },
            },
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return Result.Failure<string>(IdentityAccountErrors.UsernameAlreadyExists);
        }

        if (response.StatusCode != HttpStatusCode.Created)
        {
            logger.LogError(
                "Keycloak user creation failed with status {StatusCode}.",
                response.StatusCode);
            return Result.Failure<string>(IdentityAccountErrors.ProvisioningFailed);
        }

        var userId = response.Headers.Location?.Segments.LastOrDefault()?.Trim('/');
        return string.IsNullOrWhiteSpace(userId)
            ? Result.Failure<string>(IdentityAccountErrors.ProvisioningFailed)
            : Result.Success(userId);
    }

    private async Task<bool> AssignRealmRoleAsync(
        string token,
        string userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        using var getRoleRequest = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/roles/{Uri.EscapeDataString(roleName)}",
            token);
        using var getRoleResponse = await httpClient.SendAsync(
            getRoleRequest,
            cancellationToken);
        if (!getRoleResponse.IsSuccessStatusCode)
        {
            return false;
        }

        var role = await getRoleResponse.Content.ReadFromJsonAsync<KeycloakRoleResponse>(
            cancellationToken);
        if (role is null)
        {
            return false;
        }

        using var assignRequest = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm",
            token);
        assignRequest.Content = JsonContent.Create(new[] { role });
        using var assignResponse = await httpClient.SendAsync(
            assignRequest,
            cancellationToken);
        return assignResponse.IsSuccessStatusCode;
    }

    private async Task CompensateUserCreationAsync(string token, string userId)
    {
        try
        {
            using var request = CreateAuthorizedRequest(
                HttpMethod.Delete,
                $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/users/{Uri.EscapeDataString(userId)}",
                token);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var response = await httpClient.SendAsync(request, timeout.Token);
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
            {
                logger.LogCritical(
                    "Failed to compensate Keycloak user creation for {KeycloakUserId}; status {StatusCode}.",
                    userId,
                    response.StatusCode);
            }
        }
        catch (Exception exception)
        {
            logger.LogCritical(
                exception,
                "Compensation threw while deleting Keycloak user {KeycloakUserId}.",
                userId);
        }
    }

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        string url,
        string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private sealed record KeycloakTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken);

    private sealed record KeycloakUserResponse(
        string Id,
        string Username,
        Dictionary<string, string[]>? Attributes);

    private sealed record KeycloakRoleResponse(
        string Id,
        string Name,
        string? Description,
        bool Composite,
        bool ClientRole,
        string? ContainerId);
}
