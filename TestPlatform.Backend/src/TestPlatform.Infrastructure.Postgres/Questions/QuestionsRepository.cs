using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Questions;
using TestPlatform.Core.Questions;

namespace TestPlatform.Infrastructure.Postgres.Questions;

public class QuestionsRepository : IQuestionsRepository
{
    private readonly TestPlatformDbContext _context;

    public QuestionsRepository(TestPlatformDbContext context) => _context = context;

    public async Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Questions
            .Include(q => q.Tags)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Questions.AnyAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Question question, CancellationToken cancellationToken)
        => await _context.Questions.AddAsync(question, cancellationToken);
}