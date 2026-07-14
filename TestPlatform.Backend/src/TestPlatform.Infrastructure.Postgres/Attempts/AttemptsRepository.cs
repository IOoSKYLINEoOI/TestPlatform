using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Infrastructure.Postgres.Attempts;

public class AttemptsRepository : IAttemptsRepository
{
    private readonly TestPlatformDbContext _context;

    public AttemptsRepository(TestPlatformDbContext context) => _context = context;


    public async Task<Attempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Attempts
            .Include(x => x.AttemptAnswers)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Attempts.AnyAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Attempt attempt, CancellationToken cancellationToken)
        => await _context.Attempts.AddAsync(attempt, cancellationToken);
}