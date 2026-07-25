using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Files;
using TestPlatform.Application.Questions;
using TestPlatform.Application.Tags;
using TestPlatform.Application.Tests;
using TestPlatform.Application.Users;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Exams;
using TestPlatform.Core.Files;
using TestPlatform.Core.Questions;
using TestPlatform.Core.Tests;
using TestPlatform.Core.Users;

namespace TestPlatform.Infrastructure.Postgres;

public class TestPlatformDbContext(DbContextOptions<TestPlatformDbContext> options)
    : DbContext(options),
        ITagsReadDbContext,
        IQuestionsReadDbContext,
        ITestsReadDbContext,
        IExamsReadDbContext,
        IFileAssetsReadDbContext,
        IAttemptsReadDbContext,
        IUsersReadDbContext
{
    public DbSet<Attempt> Attempts => Set<Attempt>();

    public IQueryable<Attempt> ReadAttempts => Attempts.AsNoTracking().AsQueryable();

    public DbSet<Exam> Exams => Set<Exam>();

    public IQueryable<Exam> ReadExams => Exams.AsNoTracking().AsQueryable();

    public DbSet<Test> Tests => Set<Test>();

    public IQueryable<Test> ReadTests => Tests.AsNoTracking().AsQueryable();

    public DbSet<Question> Questions => Set<Question>();

    public IQueryable<Question> ReadQuestions => Questions.AsNoTracking().AsQueryable();

    public DbSet<Tag> Tags => Set<Tag>();

    public IQueryable<Tag> ReadTags => Tags.AsNoTracking().AsQueryable();

    public DbSet<User> Users => Set<User>();

    public IQueryable<User> ReadUsers => Users.AsNoTracking().AsQueryable();

    public DbSet<FileAsset> FileAssets => Set<FileAsset>();

    public IQueryable<FileAsset> ReadFileAssets => FileAssets.AsNoTracking().AsQueryable();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestPlatformDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
