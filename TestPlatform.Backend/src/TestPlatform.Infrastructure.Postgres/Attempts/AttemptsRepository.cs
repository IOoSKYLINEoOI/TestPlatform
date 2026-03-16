using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Core.Attempts;
using TestPlatform.Infrastructure.Postgres.Attempts.Entities;

namespace TestPlatform.Infrastructure.Postgres.Attempts;

public class AttemptsRepository : IAttemptsRepository
{
    private readonly TestPlatformDbContext _context;

    public AttemptsRepository(TestPlatformDbContext context) => _context = context;

    public async Task<Result<Guid>> AddAsync(Attempt attempt, CancellationToken cancellationToken)
    {
        var attemptEntity = MapToEntity(attempt);

        await _context.Attempts.AddAsync(attemptEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(attempt.Id);
    }

    public async Task<Result> UpdateAsync(Attempt attempt, CancellationToken cancellationToken)
    {
        var attemptEntity = await _context.Attempts.SingleOrDefaultAsync(t => t.Id == attempt.Id, cancellationToken);
        if(attemptEntity is null)
            return Result.Failure("Attempt not found");

        attemptEntity.CorrectAnswers = attempt.CorrectAnswers;
        attemptEntity.EarnedPoints = attempt.EarnedPoints;
        attemptEntity.FinishedAt = attempt.FinishedAt;
        attemptEntity.Status = attempt.Status;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Attempt>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var attemptEntity = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.Id == id)
            .SingleOrDefaultAsync(cancellationToken);
        if (attemptEntity is null)
            return Result.Failure<Attempt>("Attempt not found");

        var attempt = MapToDomain(attemptEntity);

        return Result.Success(attempt);
    }

    public Task<bool> ExistsAsync(Guid attemptId, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<Result> DeleteAsync(Guid attemptId, CancellationToken cancellationToken) => throw new NotImplementedException();

    private static AttemptEntity MapToEntity(Attempt attempt) => new AttemptEntity()
    {
        Id = attempt.Id,
        TotalQuestions = attempt.TotalQuestions,
        CorrectAnswers = attempt.CorrectAnswers,
        EarnedPoints = attempt.EarnedPoints,
        MaxPoints = attempt.MaxPoints,
        UserId = attempt.UserId,
        StartedAt = attempt.StartedAt,
        FinishedAt = attempt.FinishedAt,
        Status = attempt.Status,
        SourceId = attempt.SourceId,
    };

    private static Attempt MapToDomain(AttemptEntity entity)
    {
        return Attempt.FromPersistence(
            entity.Id,
            entity.TotalQuestions,
            entity.MaxPoints,
            entity.EarnedPoints,
            entity.CorrectAnswers,
            entity.UserId,
            entity.Status,
            entity.StartedAt,
            entity.FinishedAt,
            entity.Type,
            entity.SourceId);
    }
}