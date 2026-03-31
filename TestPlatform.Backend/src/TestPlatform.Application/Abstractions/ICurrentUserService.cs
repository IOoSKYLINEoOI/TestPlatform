using CSharpFunctionalExtensions;

namespace TestPlatform.Application.Abstractions;

public interface ICurrentUserService
{
    string? KeycloakId { get; }

    Task<Result<Guid>> GetUserIdAsync(CancellationToken cancellationToken);
}