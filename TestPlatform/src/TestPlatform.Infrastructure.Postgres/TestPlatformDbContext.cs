using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Categories;
using TestPlatform.Contracts.Categories.DTOs;
using TestPlatform.Infrastructure.Postgres.Categories;
using TestPlatform.Infrastructure.Postgres.TestAttempts.Entities;
using TestPlatform.Infrastructure.Postgres.Tests;
using TestPlatform.Infrastructure.Postgres.Tests.Configurations;
using TestPlatform.Infrastructure.Postgres.Tests.Entities;

namespace TestPlatform.Infrastructure.Postgres;

public class TestPlatformDbContext(DbContextOptions<TestPlatformDbContext> options) : DbContext(options)
{
    public DbSet<CategoryEntity> Categories { get; set; }

    public DbSet<TestEntity> Tests { get; set; }

    public DbSet<TestAttemptEntity> TestAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CategoriesConfiguration());

        modelBuilder.ApplyConfiguration(new TestsConfiguration());
        modelBuilder.ApplyConfiguration(new QuestionsConfiguration());
        modelBuilder.ApplyConfiguration(new AnswersConfiguration());
    }
}