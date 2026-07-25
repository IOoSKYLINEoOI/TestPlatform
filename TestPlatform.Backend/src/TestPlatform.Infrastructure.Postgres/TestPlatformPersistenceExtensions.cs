using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Files;
using TestPlatform.Application.Questions;
using TestPlatform.Application.Tags;
using TestPlatform.Application.Tests;
using TestPlatform.Application.Users;
using TestPlatform.Infrastructure.Postgres.Attempts;
using TestPlatform.Infrastructure.Postgres.Exams;
using TestPlatform.Infrastructure.Postgres.Files;
using TestPlatform.Infrastructure.Postgres.Questions;
using TestPlatform.Infrastructure.Postgres.Seeding;
using TestPlatform.Infrastructure.Postgres.Tests;
using TestPlatform.Infrastructure.Postgres.Users;

namespace TestPlatform.Infrastructure.Postgres;

public static class TestPlatformPersistenceExtensions
{
    public static IServiceCollection AddTestPlatformPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TestPlatformDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("TestPlatformContextPostgreSQL")
                              ?? throw new InvalidOperationException("Connection string 'TestPlatformContextPostgreSQL' not found.")));

        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<ITagsReadDbContext, TestPlatformDbContext>();

        services.AddScoped<IQuestionsRepository, QuestionsRepository>();
        services.AddScoped<IQuestionsReadDbContext, TestPlatformDbContext>();

        services.AddScoped<ITestsRepository, TestsRepository>();
        services.AddScoped<ITestsReadDbContext, TestPlatformDbContext>();

        services.AddScoped<IExamsRepository, ExamsRepository>();
        services.AddScoped<IExamsReadDbContext, TestPlatformDbContext>();

        services.AddScoped<IAttemptsRepository, AttemptsRepository>();
        services.AddScoped<IAttemptExpirationService, AttemptExpirationService>();
        services.AddScoped<IAttemptStartStore, AttemptStartStore>();
        services.AddScoped<IAttemptsReadDbContext, TestPlatformDbContext>();

        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IUsersReadDbContext, TestPlatformDbContext>();

        services.AddScoped<IFileAssetsRepository, FileAssetsRepository>();
        services.AddScoped<IFileAssetsReadDbContext, TestPlatformDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DevelopmentDataSeeder>();

        return services;
    }
}
