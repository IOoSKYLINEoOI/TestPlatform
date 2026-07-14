using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts;

public interface IAttemptsRepository
{
    Task<Attempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Attempt attempt, CancellationToken cancellationToken);
}