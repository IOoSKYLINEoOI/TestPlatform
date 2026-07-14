using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Users.DTOs;
using TestPlatform.Core.Users;
using TestPlatform.Infrastructure.Postgres;

namespace TestPlatform.Web.Middleware;

public class EnsureUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EnsureUserMiddleware> _logger;

    public EnsureUserMiddleware(
        RequestDelegate next,
        ILogger<EnsureUserMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IUsersReadDbContext usersReadDbContext,
        UnitOfWork unitOfWork,
        IUsersRepository usersRepository)
    {
        var principal = context.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        const string currentUserKey = "CurrentUser";

        if (context.Items.ContainsKey(currentUserKey))
        {
            await _next(context);
            return;
        }

        var keycloakId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var tabNumber = principal.Identity?.Name;

        if (string.IsNullOrWhiteSpace(keycloakId) ||
            string.IsNullOrWhiteSpace(tabNumber))
        {
            _logger.LogWarning("Missing required claims for user provisioning");
            await _next(context);
            return;
        }

        var userInfo = await usersReadDbContext.ReadUsers
            .AsNoTracking()
            .Where(x => x.KeycloakId == keycloakId)
            .Select(x => new
            {
                x.Id,
                x.KeycloakId,
                x.TabNumber,
            })
            .FirstOrDefaultAsync(context.RequestAborted);

        if (userInfo is null)
        {
            var createResult = User.Create(keycloakId, tabNumber);

            if (createResult.IsFailure)
            {
                _logger.LogError("Failed to create user: {Error}", createResult.Error);
                await _next(context);
                return;
            }

            await usersRepository.AddAsync(createResult.Value, context.RequestAborted);
            await unitOfWork.SaveChangesAsync(context.RequestAborted);

            userInfo = new
            {
                createResult.Value.Id,
                createResult.Value.KeycloakId,
                createResult.Value.TabNumber,
            };
        }

        context.Items[currentUserKey] = new CurrentUserDto(
            userInfo.Id,
            userInfo.KeycloakId,
            userInfo.TabNumber,
            principal.IsInRole("Admin"));

        await _next(context);
    }
}