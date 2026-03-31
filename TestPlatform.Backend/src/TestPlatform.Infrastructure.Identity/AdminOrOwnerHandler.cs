using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TestPlatform.Infrastructure.Identity;

public class AdminOrOwnerHandler 
    : AuthorizationHandler<AdminOrOwnerRequirement, Guid>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminOrOwnerRequirement requirement,
        Guid resourceUserId)
    {
        // 1. Админ
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 2. Owner
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userId, out var currentUserId))
        {
            if (currentUserId == resourceUserId)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}