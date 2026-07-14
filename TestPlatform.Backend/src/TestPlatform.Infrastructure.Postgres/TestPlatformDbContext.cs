using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Questions;
using TestPlatform.Application.Tags;
using TestPlatform.Application.Tests;
using TestPlatform.Application.Users;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Exams;
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
        IAttemptsReadDbContext,
        IUsersReadDbContext
{
    public DbSet<Attempt> Attempts { get; set; }

    public IQueryable<Attempt> ReadAttempts => Attempts.AsNoTracking().AsQueryable();

    public DbSet<Exam> Exams { get; set; }

    public IQueryable<Exam> ReadExams => Exams.AsNoTracking().AsQueryable();

    public DbSet<Test> Tests { get; set; }

    public IQueryable<Test> ReadTests => Tests.AsNoTracking().AsQueryable();

    public DbSet<Question> Questions { get; set; }

    public IQueryable<Question> ReadQuestions => Questions.AsNoTracking().AsQueryable();

    public DbSet<Tag> Tags { get; set; }

    public IQueryable<Tag> ReadTags => Tags.AsNoTracking().AsQueryable();

    public DbSet<User> Users { get; set; }

    public IQueryable<User> ReadUsers => Users.AsNoTracking().AsQueryable();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestPlatformDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}