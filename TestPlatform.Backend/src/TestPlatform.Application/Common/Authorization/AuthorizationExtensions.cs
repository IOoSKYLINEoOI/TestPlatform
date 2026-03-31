using CSharpFunctionalExtensions;

namespace TestPlatform.Application.Common.Authorization;

public static class AuthorizationExtensions
{
    public static Result EnsureOwner(Guid ownerId, Guid currentUserId)
    {
        return ownerId == currentUserId
            ? Result.Success()
            : Result.Failure("forbidden");
    }
}