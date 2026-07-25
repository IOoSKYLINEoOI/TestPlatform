using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Microsoft.Extensions.Options;
using TestPlatform.Application.Files;

namespace TestPlatform.Infrastructure.Files.ObjectStorage.Minio;

public static class TestPlatformMinioObjectStorageExtensions
{
    public static IServiceCollection AddTestPlatformMinioObjectStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<MinioStorageOptions>()
            .Bind(configuration.GetSection("ImageStorage:Minio"))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), "MinIO endpoint is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.PublicEndpoint), "MinIO public endpoint is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.AccessKey), "MinIO access key is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "MinIO secret key is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.BucketName), "MinIO bucket name is required.")
            .Validate(options => options.PresignedUrlExpirySeconds > 0, "Presigned URL expiry must be positive.")
            .ValidateOnStart();

        services.AddSingleton<IMinioClient>(provider =>
        {
            var minioOptions = provider.GetRequiredService<IOptions<MinioStorageOptions>>().Value;
            var client = new MinioClient()
                .WithEndpoint(minioOptions.Endpoint)
                .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey);

            if (minioOptions.UseSsl)
            {
                client = client.WithSSL();
            }

            return client.Build();
        });

        services.AddScoped<IObjectStorage, MinioObjectStorage>();

        return services;
    }
}
