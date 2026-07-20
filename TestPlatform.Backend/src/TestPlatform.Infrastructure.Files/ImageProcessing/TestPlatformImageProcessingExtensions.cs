using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Application.Files;

namespace TestPlatform.Infrastructure.Files.ImageProcessing;

public static class TestPlatformImageProcessingExtensions
{
    public static IServiceCollection AddTestPlatformImageProcessing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ImageProcessingOptions>(configuration.GetSection("ImageStorage"));
        services.AddScoped<IImageProcessor, ImageSharpImageProcessor>();

        return services;
    }
}
