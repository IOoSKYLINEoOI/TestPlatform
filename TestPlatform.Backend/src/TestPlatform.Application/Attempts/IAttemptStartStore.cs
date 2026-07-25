using CSharpFunctionalExtensions;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts;

public interface IAttemptStartStore
{
    Task<Attempt?> FindByRequestIdAsync(
        Guid userId,
        Guid requestId,
        CancellationToken cancellationToken);

    Task<Result<AttemptStartStoreResult>> AddAsync(
        Attempt attempt,
        int? attemptsLimit,
        CancellationToken cancellationToken);
}

public record AttemptStartStoreResult(Attempt Attempt, bool Created);
