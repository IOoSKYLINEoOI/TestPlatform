using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Application.Users;

public interface ICurrentUserAccessor
{
    CurrentUserDto? User { get; }
}