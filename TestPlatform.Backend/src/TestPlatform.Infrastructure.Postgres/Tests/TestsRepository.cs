using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Tests;
using TestPlatform.Core.Tests;

namespace TestPlatform.Infrastructure.Postgres.Tests;

public class TestsRepository : ITestsRepository
{
    private readonly TestPlatformDbContext _context;

    public TestsRepository(TestPlatformDbContext context) => _context = context;


    public async Task<Test?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Tests
            .Include(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Test test, CancellationToken cancellationToken)
        => await _context.Tests.AddAsync(test, cancellationToken);
}
