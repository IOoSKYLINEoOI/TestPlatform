using System.Security.Claims;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Users.DTOs;
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

    public async Task InvokeAsync(HttpContext context, IUsersReadRepository usersReadRepository, IUsersRepository usersRepository)
    {
        var claims = context.User;

        if (!claims.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        var keycloakId = claims.FindFirst("sub")?.Value ?? claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tabNumber = claims.FindFirst("preferred_username")?.Value;

        if (string.IsNullOrEmpty(keycloakId) || string.IsNullOrEmpty(tabNumber))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: missing claims.");
            return;
        }

        var userDto = await usersReadRepository.GetByKeycloakIdAsync(keycloakId, context.RequestAborted);

        if (userDto == null)
        {
            var newUserResult = User.Create(keycloakId, tabNumber);

            if (newUserResult.IsFailure)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Failed to create user.");
                return;
            }

            var addResult = await usersRepository.AddAsync(newUserResult.Value, context.RequestAborted);

            if (addResult.IsFailure)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Failed to save new user.");
                return;
            }

            userDto = new CurrentUserResponse(
                Id: newUserResult.Value.Id,
                KeycloakId: newUserResult.Value.KeycloakId,
                TabNumber: newUserResult.Value.TabNumber);
        }

        context.Items["CurrentUser"] = userDto;

        await _next(context);
    }

}
