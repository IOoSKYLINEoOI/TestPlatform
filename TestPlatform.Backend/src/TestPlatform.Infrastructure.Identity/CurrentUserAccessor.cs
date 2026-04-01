using Microsoft.AspNetCore.Http;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Infrastructure.Identity;

public class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUserDto? User
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return null;

            return context.Items["CurrentUser"] as CurrentUserDto;
        }
    }
}