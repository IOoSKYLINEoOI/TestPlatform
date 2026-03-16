using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Application.Abstractions;

namespace TestPlatform.Infrastructure.FileStorage;

public static class TestPlatformFileStorageExtensions
{
    public static IServiceCollection AddTestPlatformFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ImageStorageOptions>(configuration.GetSection("ImageStorage"));

        services.AddScoped<IImageStorageService, ImageStorageService>();

        return services;
    }
}