using TestPlatform.Application.Abstractions;

namespace TestPlatform.Infrastructure.Postgres;

public class UnitOfWork : IUnitOfWork
{
    private readonly TestPlatformDbContext _context;

    public UnitOfWork(TestPlatformDbContext context)
    {
        _context = context;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _context.SaveChangesAsync(ct);
}