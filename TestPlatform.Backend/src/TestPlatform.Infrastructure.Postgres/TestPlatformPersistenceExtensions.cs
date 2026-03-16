using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Questions;
using TestPlatform.Application.Tags;
using TestPlatform.Application.Tests;
using TestPlatform.Infrastructure.Postgres.Attempts;
using TestPlatform.Infrastructure.Postgres.Exams;
using TestPlatform.Infrastructure.Postgres.Questions;
using TestPlatform.Infrastructure.Postgres.Tags;
using TestPlatform.Infrastructure.Postgres.Tests;

namespace TestPlatform.Infrastructure.Postgres;

public static class TestPlatformPersistenceExtensions
{
    public static IServiceCollection AddTestPlatformPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TestPlatformDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("TestPlatformContextPostgreSQL")
                              ?? throw new InvalidOperationException("Connection string 'TestPlatformContextPostgreSQL' not found.")));

        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<ITagsReadRepository, TagsReadRepository>();

        services.AddScoped<IQuestionsRepository, QuestionsRepository>();
        services.AddScoped<IQuestionsReadRepository, QuestionsReadRepository>();

        services.AddScoped<ITestsRepository, TestsRepository>();
        services.AddScoped<ITestsReadRepository, TestsReadRepository>();

        services.AddScoped<IAttemptsRepository, AttemptsRepository>();
        services.AddScoped<IAttemptsReadRepository, AttemptsReadRepository>();

        services.AddScoped<IExamsReadRepository, ExamsReadRepository>();

        return services;
    }
}