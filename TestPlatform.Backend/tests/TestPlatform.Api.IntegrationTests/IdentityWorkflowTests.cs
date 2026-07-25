using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Api.IntegrationTests.Infrastructure;
using TestPlatform.Contracts.Users.DTOs;
using TestPlatform.Infrastructure.Postgres;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class IdentityWorkflowTests(TestPlatformWebApplicationFactory factory)
    : IClassFixture<TestPlatformWebApplicationFactory>
{
    [Fact]
    public async Task Admin_CanCreateAccountWithTemporaryPasswordAndRole()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "account-admin");
        client.DefaultRequestHeaders.Add("X-Test-Employee-Number", "ACCOUNT-ADMIN");

        var response = await client.PostAsJsonAsync(
            "/users",
            new CreateUserAccountRequest(
                $"employee.{Guid.NewGuid():N}",
                $"EMP-{Guid.NewGuid():N}",
                "Temporary-Password-123!",
                "Employee"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<CreateUserAccountResponse>();
        Assert.NotNull(account);
        Assert.Equal("Employee", account.Role);
        Assert.False(string.IsNullOrWhiteSpace(account.IdentityProviderUserId));
    }

    [Fact]
    public async Task NonAdmin_CannotCreateAccount()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/users",
            new CreateUserAccountRequest(
                "employee.login",
                "EMP-FORBIDDEN",
                "Temporary-Password-123!",
                "Employee"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AccountCreation_RejectsCyrillicUsername()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var response = await client.PostAsJsonAsync(
            "/users",
            new CreateUserAccountRequest(
                "сотрудник",
                "EMP-VALIDATION",
                "Temporary-Password-123!",
                "Employee"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FirstLogin_ProvisionsUser_AndRepeatedLoginReturnsSameIdentity()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "identity-repeat");
        client.DefaultRequestHeaders.Add("X-Test-Employee-Number", "EMP-REPEAT");

        var first = await client.GetFromJsonAsync<CurrentUserResponse>("/users/me");
        var second = await client.GetFromJsonAsync<CurrentUserResponse>("/users/me");

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal("EMP-REPEAT", first.EmployeeNumber);
        Assert.False(first.IsAdmin);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
        Assert.Equal(
            1,
            await dbContext.Users.CountAsync(user => user.KeycloakId == "identity-repeat"));
    }

    [Fact]
    public async Task ChangedEmployeeNumber_ForExistingSubject_IsRejected()
    {
        using var initialClient = factory.CreateClient();
        initialClient.DefaultRequestHeaders.Add("X-Test-Subject", "identity-mismatch");
        initialClient.DefaultRequestHeaders.Add("X-Test-Employee-Number", "EMP-ORIGINAL");
        var initialResponse = await initialClient.GetAsync("/users/me");
        Assert.Equal(HttpStatusCode.OK, initialResponse.StatusCode);

        using var changedClient = factory.CreateClient();
        changedClient.DefaultRequestHeaders.Add("X-Test-Subject", "identity-mismatch");
        changedClient.DefaultRequestHeaders.Add("X-Test-Employee-Number", "EMP-CHANGED");
        var changedResponse = await changedClient.GetAsync("/users/me");

        Assert.Equal(HttpStatusCode.Forbidden, changedResponse.StatusCode);
        await AssertProblemCodeAsync(
            changedResponse,
            "identity.employee_number_mismatch");
    }

    [Fact]
    public async Task MissingRequiredClaims_ReturnsProblemDetails()
    {
        using var missingSubjectClient = factory.CreateClient();
        missingSubjectClient.DefaultRequestHeaders.Add("X-Test-Omit-Subject", "true");
        var missingSubjectResponse = await missingSubjectClient.GetAsync("/users/me");
        Assert.Equal(HttpStatusCode.Forbidden, missingSubjectResponse.StatusCode);
        await AssertProblemCodeAsync(
            missingSubjectResponse,
            "identity.required_claim_missing");

        using var missingEmployeeClient = factory.CreateClient();
        missingEmployeeClient.DefaultRequestHeaders.Add("X-Test-Subject", "identity-no-employee");
        missingEmployeeClient.DefaultRequestHeaders.Add("X-Test-Omit-Employee-Number", "true");
        var missingEmployeeResponse = await missingEmployeeClient.GetAsync("/users/me");
        Assert.Equal(HttpStatusCode.Forbidden, missingEmployeeResponse.StatusCode);
        await AssertProblemCodeAsync(
            missingEmployeeResponse,
            "identity.required_claim_missing");
    }

    [Fact]
    public async Task AdminRole_IsExposedThroughCurrentIdentity()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "identity-admin");
        client.DefaultRequestHeaders.Add("X-Test-Employee-Number", "EMP-ADMIN");

        var currentUser = await client.GetFromJsonAsync<CurrentUserResponse>("/users/me");

        Assert.NotNull(currentUser);
        Assert.True(currentUser.IsAdmin);
        Assert.Equal("EMP-ADMIN", currentUser.EmployeeNumber);
    }

    [Fact]
    public async Task IdentityClaims_AreNormalizedBeforeProvisioning()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "  identity-normalized  ");
        client.DefaultRequestHeaders.Add("X-Test-Employee-Number", "  EMP-NORMALIZED  ");

        var currentUser = await client.GetFromJsonAsync<CurrentUserResponse>("/users/me");

        Assert.NotNull(currentUser);
        Assert.Equal("EMP-NORMALIZED", currentUser.EmployeeNumber);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
        var persisted = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.Id == currentUser.Id);
        Assert.Equal("identity-normalized", persisted.KeycloakId);
        Assert.Equal("EMP-NORMALIZED", persisted.EmployeeNumber);
    }

    private static async Task AssertProblemCodeAsync(
        HttpResponseMessage response,
        string expectedCode)
    {
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(expectedCode, document.RootElement.GetProperty("code").GetString());
        Assert.True(document.RootElement.TryGetProperty("traceId", out _));
    }
}
