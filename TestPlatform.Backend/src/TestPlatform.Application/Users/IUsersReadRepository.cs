using CSharpFunctionalExtensions;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Application.Users;

public interface IUsersReadRepository
{
    Task<CurrentUserResponse?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string keycloakId, CancellationToken cancellationToken);
}