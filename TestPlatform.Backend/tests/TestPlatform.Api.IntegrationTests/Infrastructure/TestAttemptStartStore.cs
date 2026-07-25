using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;
using TestPlatform.Infrastructure.Postgres;

namespace TestPlatform.Api.IntegrationTests.Infrastructure;

public sealed class TestAttemptStartStore(TestPlatformDbContext context) : IAttemptStartStore
{
    public Task<Attempt?> FindByRequestIdAsync(
        Guid userId,
        Guid requestId,
        CancellationToken cancellationToken) => context.Attempts
            .Include(x => x.QuestionSelections)
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.RequestId == requestId,
                cancellationToken);

    public async Task<Result<AttemptStartStoreResult>> AddAsync(
        Attempt attempt,
        int? attemptsLimit,
        CancellationToken cancellationToken)
    {
        var existing = await FindByRequestIdAsync(
            attempt.UserId,
            attempt.RequestId,
            cancellationToken);
        if (existing is not null)
        {
            return existing.Type == attempt.Type && existing.SourceId == attempt.SourceId
                ? Result.Success(new AttemptStartStoreResult(existing, false))
                : Result.Failure<AttemptStartStoreResult>("attempt.request_id_conflict");
        }

        var attempts = await context.Attempts
            .Where(x => x.UserId == attempt.UserId
                && x.Type == attempt.Type
                && x.SourceId == attempt.SourceId)
            .ToListAsync(cancellationToken);
        if (attemptsLimit.HasValue
            && attempts.Count(x => x.Status != AttemptStatus.CANCELLED) >= attemptsLimit.Value)
        {
            return Result.Failure<AttemptStartStoreResult>("exam.attempts_limit_reached");
        }

        var numberResult = attempt.AssignAttemptNumber(
            attempts.Select(x => x.AttemptNumber).DefaultIfEmpty().Max() + 1);
        if (numberResult.IsFailure)
        {
            return Result.Failure<AttemptStartStoreResult>(numberResult.Error);
        }

        context.Attempts.Add(attempt);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(new AttemptStartStoreResult(attempt, true));
    }
}
