using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Infrastructure.Postgres.Attempts;

public sealed class AttemptStartStore(TestPlatformDbContext context) : IAttemptStartStore
{
    public Task<Attempt?> FindByRequestIdAsync(
        Guid userId,
        Guid requestId,
        CancellationToken cancellationToken) => context.Attempts
            .AsNoTracking()
            .Include(x => x.QuestionSelections)
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.RequestId == requestId,
                cancellationToken);

    public async Task<Result<AttemptStartStoreResult>> AddAsync(
        Attempt attempt,
        int? attemptsLimit,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var requestLockKey = $"attempt-request:{attempt.UserId:N}:{attempt.RequestId:N}";
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({requestLockKey}, 0))",
            cancellationToken);

        var existing = await context.Attempts
            .Include(x => x.QuestionSelections)
            .FirstOrDefaultAsync(
                x => x.UserId == attempt.UserId && x.RequestId == attempt.RequestId,
                cancellationToken);
        if (existing is not null)
        {
            if (existing.Type != attempt.Type || existing.SourceId != attempt.SourceId)
            {
                return Result.Failure<AttemptStartStoreResult>("attempt.request_id_conflict");
            }

            await transaction.CommitAsync(cancellationToken);
            return Result.Success(new AttemptStartStoreResult(existing, false));
        }

        var sourceLockKey = $"attempt-source:{attempt.UserId:N}:{attempt.Type}:{attempt.SourceId:N}";
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({sourceLockKey}, 0))",
            cancellationToken);

        var usedAttempts = await context.Attempts.CountAsync(
            x => x.UserId == attempt.UserId
                && x.Type == attempt.Type
                && x.SourceId == attempt.SourceId
                && x.Status != AttemptStatus.CANCELLED,
            cancellationToken);

        if (attemptsLimit.HasValue && usedAttempts >= attemptsLimit.Value)
        {
            return Result.Failure<AttemptStartStoreResult>("exam.attempts_limit_reached");
        }

        var lastAttemptNumber = await context.Attempts
            .Where(x => x.UserId == attempt.UserId
                && x.Type == attempt.Type
                && x.SourceId == attempt.SourceId)
            .MaxAsync(x => (int?)x.AttemptNumber, cancellationToken) ?? 0;

        var numberResult = attempt.AssignAttemptNumber(lastAttemptNumber + 1);
        if (numberResult.IsFailure)
        {
            return Result.Failure<AttemptStartStoreResult>(numberResult.Error);
        }

        await context.Attempts.AddAsync(attempt, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success(new AttemptStartStoreResult(attempt, true));
    }
}
