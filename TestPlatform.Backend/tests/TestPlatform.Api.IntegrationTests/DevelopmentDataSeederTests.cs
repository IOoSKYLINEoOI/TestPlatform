using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TestPlatform.Infrastructure.Postgres;
using TestPlatform.Infrastructure.Postgres.Seeding;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class DevelopmentDataSeederTests
{
    [Fact]
    public async Task SeedAsync_RepeatedRun_IsIdempotentAndCreatesConnectedData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestPlatformDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new TestPlatformDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        var seeder = new DevelopmentDataSeeder(
            dbContext,
            NullLogger<DevelopmentDataSeeder>.Instance);

        var first = await seeder.SeedAsync(CancellationToken.None);
        var second = await seeder.SeedAsync(CancellationToken.None);

        Assert.True(first.Created);
        Assert.False(second.Created);
        Assert.Equal(20, await dbContext.Users.CountAsync());
        Assert.Equal(12, await dbContext.Tags.CountAsync());
        Assert.Equal(150, await dbContext.Questions.CountAsync());
        Assert.Equal(20, await dbContext.Tests.CountAsync());
        Assert.Equal(8, await dbContext.Exams.CountAsync());
        Assert.Equal(240, await dbContext.Attempts.CountAsync());
        Assert.True(await dbContext.Attempts
            .Select(attempt => attempt.Status)
            .Distinct()
            .CountAsync() >= 5);

        var publishedExam = await dbContext.Exams
            .AsNoTracking()
            .Include(exam => exam.Sections)
            .ThenInclude(section => section.Questions)
            .FirstAsync();
        Assert.Equal(5, publishedExam.TotalQuestions);
        Assert.Equal(30, publishedExam.Sections.Single().Questions.Count);
    }
}
