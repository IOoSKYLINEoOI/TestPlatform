using Microsoft.EntityFrameworkCore;
using TestPlatform.Infrastructure.Postgres.Attempts.Configurations;
using TestPlatform.Infrastructure.Postgres.Attempts.Entities;
using TestPlatform.Infrastructure.Postgres.Exams.Configuration;
using TestPlatform.Infrastructure.Postgres.Exams.Entities;
using TestPlatform.Infrastructure.Postgres.Questions.Configurations;
using TestPlatform.Infrastructure.Postgres.Questions.Entities;
using TestPlatform.Infrastructure.Postgres.Tags.Configurations;
using TestPlatform.Infrastructure.Postgres.Tags.Entities;
using TestPlatform.Infrastructure.Postgres.Tests.Configurations;
using TestPlatform.Infrastructure.Postgres.Tests.Entities;
using TestPlatform.Infrastructure.Postgres.Users.Configurations;
using TestPlatform.Infrastructure.Postgres.Users.Entities;

namespace TestPlatform.Infrastructure.Postgres;

public class TestPlatformDbContext(DbContextOptions<TestPlatformDbContext> options) : DbContext(options)
{
    public DbSet<AttemptEntity> Attempts { get; set; }

    public DbSet<ExamEntity> Exams { get; set; }

    public DbSet<QuestionEntity> Questions { get; set; }

    public DbSet<AnswerOptionEntity> AnswerOptions { get; set; }

    public DbSet<TagEntity> Tags { get; set; }

    public DbSet<TestEntity> Tests { get; set; }

    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AttemptsConfiguration());

        modelBuilder.ApplyConfiguration(new ExamsConfiguration());

        modelBuilder.ApplyConfiguration(new QuestionsConfiguration());
        modelBuilder.ApplyConfiguration(new AnswerOptionsConfiguration());

        modelBuilder.ApplyConfiguration(new TagsConfiguration());

        modelBuilder.ApplyConfiguration(new TestsConfiguration());

        modelBuilder.ApplyConfiguration(new UsersConfiguration());
    }
}