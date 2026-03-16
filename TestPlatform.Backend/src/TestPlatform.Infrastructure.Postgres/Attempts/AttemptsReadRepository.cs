using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Infrastructure.Postgres.Attempts;

public class AttemptsReadRepository : IAttemptsReadRepository
{
    private readonly TestPlatformDbContext _context;

    public AttemptsReadRepository(TestPlatformDbContext context) => _context = context;

    public async Task<AttemptResponse?> ReadAttemptByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Attempts
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (entity is null) return null;

        return new AttemptResponse(
            Id: entity.Id,
            TotalQuestions: entity.TotalQuestions,
            CorrectAnswers: entity.CorrectAnswers,
            EarnedPoints: entity.EarnedPoints,
            MaxPoints: entity.MaxPoints,
            UserId: entity.UserId,
            StartedAt: entity.StartedAt,
            FinishedAt: entity.FinishedAt,
            Status: (AttemptStatusDto)entity.Status,
            Type: (AttemptTypeDto)entity.Type,
            SourceId: entity.SourceId);
    }

    public Task<List<AttemptResponse>> ReadAllAttemptByUserIdAsync(Guid userId, CancellationToken cancellationToken) => throw new NotImplementedException();
}