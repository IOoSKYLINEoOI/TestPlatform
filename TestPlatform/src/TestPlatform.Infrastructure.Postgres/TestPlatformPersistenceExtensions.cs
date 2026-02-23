using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Application.Tags;
using TestPlatform.Infrastructure.Postgres.Tags;

namespace TestPlatform.Infrastructure.Postgres;

public static class TestPlatformPersistenceExtensions
{
    public static IServiceCollection AddTestPlatformPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TestPlatformDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("TestPlatformContextPostgreSQL")
                              ?? throw new InvalidOperationException("Connection string 'TestPlatformContextPostgreSQL' not found.")));

        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<IReadTagsRepository, ReadTagsRepository>();

        return services;
    }
}