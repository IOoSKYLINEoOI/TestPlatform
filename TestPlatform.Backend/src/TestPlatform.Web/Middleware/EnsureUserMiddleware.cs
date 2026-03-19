using TestPlatform.Application.Users;
using TestPlatform.Core.Users;

namespace TestPlatform.Web.Middleware;

public class EnsureUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EnsureUserMiddleware> _logger;

    public EnsureUserMiddleware(RequestDelegate next, ILogger<EnsureUserMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUsersReadRepository readRepository, IUsersRepository writeRepository)
    {
        var claims = context.User;

        if (!claims.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        var keycloakId = claims.FindFirst("sub")?.Value;
        var tabNumber = claims.FindFirst("preferred_username")?.Value;

        if (string.IsNullOrEmpty(keycloakId) || string.IsNullOrEmpty(tabNumber))
        {
            _logger.LogWarning("Missing Keycloak claims for authenticated user.");

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsync("Unauthorized: missing claims.");
            return;
        }

        var existingResult = await readRepository.ExistingAsync(keycloakId, context.RequestAborted);

        if (existingResult.IsFailure)
        {
            var newUserResult = User.Create(keycloakId, tabNumber);

            if (newUserResult.IsFailure)
            {
                _logger.LogError("Failed to create user entity for KeycloakId {KeycloakId}: {Error}", keycloakId, newUserResult.Error);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Failed to create user.");
                return;
            }

            var addResult = await writeRepository.AddAsync(newUserResult.Value, context.RequestAborted);

            if (addResult.IsFailure)
            {
                _logger.LogError("Failed to save new user {KeycloakId}: {Error}", keycloakId, addResult.Error);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Failed to save new user.");
                return;
            }

            _logger.LogInformation("Created new user with KeycloakId {KeycloakId}", keycloakId);
        }

        await _next(context);
    }
}
