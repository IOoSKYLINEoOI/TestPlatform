using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Infrastructure.Postgres.Attempts;

public class AttemptsRepository : IAttemptsRepository
{
    private readonly TestPlatformDbContext _context;

    public AttemptsRepository(TestPlatformDbContext context) => _context = context;


    public async Task<Attempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Attempts
            .Include(x => x.AttemptAnswers)
            .Include(x => x.QuestionSelections)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Attempts.AnyAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Attempt attempt, CancellationToken cancellationToken)
        => await _context.Attempts.AddAsync(attempt, cancellationToken);

    public Task<int> CountUsedAttemptsAsync(
        Guid userId,
        AttemptType type,
        Guid sourceId,
        CancellationToken cancellationToken)
        => _context.Attempts.CountAsync(
            attempt => attempt.UserId == userId
                && attempt.Type == type
                && attempt.SourceId == sourceId
                && attempt.Status != AttemptStatus.CANCELLED,
            cancellationToken);
}
