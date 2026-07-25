using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using TestPlatform.Application.Files;

namespace TestPlatform.Infrastructure.Files.ObjectStorage.Minio;

public class MinioObjectStorage : IObjectStorage
{
    private readonly IMinioClient _minioClient;
    private readonly IMinioClient _publicMinioClient;
    private readonly MinioStorageOptions _options;
    private readonly ILogger<MinioObjectStorage> _logger;

    public MinioObjectStorage(
        IMinioClient minioClient,
        IOptions<MinioStorageOptions> options,
        ILogger<MinioObjectStorage> logger)
    {
        _minioClient = minioClient;
        _options = options.Value;
        _logger = logger;

        var publicEndpoint = new Uri(_options.PublicEndpoint);
        var publicClient = new MinioClient()
            .WithEndpoint(publicEndpoint.Authority)
            .WithCredentials(_options.AccessKey, _options.SecretKey);

        if (publicEndpoint.Scheme == Uri.UriSchemeHttps)
        {
            publicClient = publicClient.WithSSL();
        }

        _publicMinioClient = publicClient.Build();
    }

    public async Task<Result> PutAsync(
        string objectKey,
        Stream stream,
        long sizeBytes,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            await EnsureBucketExists(cancellationToken);

            var args = new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(sizeBytes)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(args, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error putting object {ObjectKey} to MinIO", objectKey);
            return Result.Failure("file.save_error");
        }
    }

    public async Task<Result<Stream>> GetAsync(string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            var stream = new MemoryStream();
            var args = new GetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey)
                .WithCallbackStream(stream.CopyTo);

            await _minioClient.GetObjectAsync(args, cancellationToken);
            stream.Position = 0;

            return Result.Success<Stream>(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting object {ObjectKey} from MinIO", objectKey);
            return Result.Failure<Stream>("file.not_found");
        }
    }

    public async Task<Result> DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey);

            await _minioClient.RemoveObjectAsync(args, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting object {ObjectKey} from MinIO", objectKey);
            return Result.Failure("file.delete_error");
        }
    }

    public async Task<Result<string>> GetUrlAsync(string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey)
                .WithExpiry(_options.PresignedUrlExpirySeconds);

            var url = await _publicMinioClient.PresignedGetObjectAsync(args);
            return Result.Success(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating download URL for object {ObjectKey} in MinIO", objectKey);
            return Result.Failure<string>("file.url_error");
        }
    }

    private async Task EnsureBucketExists(CancellationToken cancellationToken)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(_options.BucketName);
        if (await _minioClient.BucketExistsAsync(existsArgs, cancellationToken))
        {
            return;
        }

        await _minioClient.MakeBucketAsync(
            new MakeBucketArgs().WithBucket(_options.BucketName),
            cancellationToken);
    }
}
