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

    public async Task InvokeAsync(
        HttpContext context,
        IUsersReadRepository usersReadRepository,
        IUsersRepository usersRepository)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        if (context.Items.ContainsKey("CurrentUser"))
        {
            await _next(context);
            return;
        }

        var keycloakId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tabNumber = user.Identity?.Name;

        if (string.IsNullOrEmpty(keycloakId) || string.IsNullOrEmpty(tabNumber))
        {
            _logger.LogWarning("Missing required claims for user provisioning");
            await _next(context);
            return;
        }

        var userDto = await usersReadRepository
            .GetByKeycloakIdAsync(keycloakId, context.RequestAborted);

        if (userDto == null)
        {
            var newUserResult = User.Create(keycloakId, tabNumber);

            if (newUserResult.IsFailure)
            {
                _logger.LogError("Failed to create user domain object");
                await _next(context);
                return;
            }

            var addResult = await usersRepository
                .AddAsync(newUserResult.Value, context.RequestAborted);

            if (addResult.IsFailure)
            {
                _logger.LogError("Failed to persist new user");
                await _next(context);
                return;
            }

            userDto = new CurrentUserDto(
                Id: newUserResult.Value.Id,
                KeycloakId: newUserResult.Value.KeycloakId,
                TabNumber: newUserResult.Value.TabNumber,
                IsAdmin: user.IsInRole("Admin"));
        }

        context.Items["CurrentUser"] = userDto;

        await _next(context);
    }
}
