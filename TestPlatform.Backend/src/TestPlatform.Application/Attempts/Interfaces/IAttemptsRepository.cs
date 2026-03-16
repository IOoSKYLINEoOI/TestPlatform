using CSharpFunctionalExtensions;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts.Interfaces;

public interface IAttemptsRepository
{
    Task<Result<Guid>> AddAsync(Attempt attempt, CancellationToken cancellationToken);

    Task<Result> UpdateAsync(Attempt attempt, CancellationToken cancellationToken);

    Task<Result<Attempt>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid attemptId, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid attemptId, CancellationToken cancellationToken);
}