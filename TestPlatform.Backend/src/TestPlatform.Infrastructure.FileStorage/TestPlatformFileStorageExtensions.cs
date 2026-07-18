using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using TestPlatform.Application.Files;

namespace TestPlatform.Infrastructure.FileStorage;

public static class TestPlatformFileStorageExtensions
{
    public static IServiceCollection AddTestPlatformFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioStorageOptions>(configuration.GetSection("ImageStorage:Minio"));

        var minioOptions = configuration.GetSection("ImageStorage:Minio").Get<MinioStorageOptions>() ?? new MinioStorageOptions();

        services.AddSingleton<IMinioClient>(_ =>
        {
            var client = new MinioClient()
                .WithEndpoint(minioOptions.Endpoint)
                .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey);

            if (minioOptions.UseSsl)
                client = client.WithSSL();

            return client.Build();
        });

        services.AddScoped<IObjectStorage, MinioObjectStorage>();

        return services;
    }
}