using Microsoft.AspNetCore.Http;
using TestPlatform.Application.Users;

namespace TestPlatform.Infrastructure.Identity;

public class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentIdentity? User
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return null;
            }

            return CurrentIdentityHttpContext.Get(context);
        }
    }
}
