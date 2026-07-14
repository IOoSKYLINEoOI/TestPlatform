using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Exams;
using TestPlatform.Core.Exams;

namespace TestPlatform.Infrastructure.Postgres.Exams;

public class ExamsRepository : IExamsRepository
{
    private readonly TestPlatformDbContext _context;

    public ExamsRepository(TestPlatformDbContext context) => _context = context;

    public async Task<Exam?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Exams
            .Include(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Exam exam, CancellationToken cancellationToken)
        => await _context.Exams.AddAsync(exam, cancellationToken);
}