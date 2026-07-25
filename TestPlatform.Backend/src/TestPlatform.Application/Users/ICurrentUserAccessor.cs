namespace TestPlatform.Application.Users;

public interface ICurrentUserAccessor
{
    CurrentIdentity? User { get; }
}
