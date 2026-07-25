using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TestPlatform.Infrastructure.Postgres;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TestPlatformDbContext>
{
    private const string ConnectionStringEnvironmentVariable =
        "ConnectionStrings__TestPlatformContextPostgreSQL";

    public TestPlatformDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
            ?? "Host=localhost;Database=testPlatformDb;Username=postgres";

        var options = new DbContextOptionsBuilder<TestPlatformDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TestPlatformDbContext(options);
    }
}
