using Microsoft.AspNetCore.Http;
using TestPlatform.Application.Users;

namespace TestPlatform.Infrastructure.Identity;

internal static class CurrentIdentityHttpContext
{
    internal const string ItemKey = "TestPlatform.CurrentIdentity";

    internal static void Set(HttpContext context, CurrentIdentity identity) =>
        context.Items[ItemKey] = identity;

    internal static CurrentIdentity? Get(HttpContext context) =>
        context.Items[ItemKey] as CurrentIdentity;
}
