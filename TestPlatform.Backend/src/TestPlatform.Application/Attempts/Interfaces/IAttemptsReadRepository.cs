using TestPlatform.Contracts.Attempts.DTOs;

namespace TestPlatform.Application.Attempts.Interfaces;

public interface IAttemptsReadRepository
{
    Task<AttemptResponse?> ReadAttemptByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<AttemptResponse>> ReadAllAttemptByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}