using Microsoft.AspNetCore.Authorization;

namespace TestPlatform.Infrastructure.Identity;

public class AdminOrOwnerRequirement : IAuthorizationRequirement
{
}