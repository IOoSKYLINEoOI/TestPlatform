using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts;

public interface IAttemptsRepository
{
    Task<Attempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Attempt attempt, CancellationToken cancellationToken);
    Task<int> CountUsedAttemptsAsync(
        Guid userId,
        AttemptType type,
        Guid sourceId,
        CancellationToken cancellationToken);
}
