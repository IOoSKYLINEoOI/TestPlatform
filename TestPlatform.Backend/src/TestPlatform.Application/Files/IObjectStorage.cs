using CSharpFunctionalExtensions;

namespace TestPlatform.Application.Files;

public interface IObjectStorage
{
    Task<Result> PutAsync(
        string objectKey,
        Stream stream,
        long sizeBytes,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<Stream>> GetAsync(string objectKey, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(string objectKey, CancellationToken cancellationToken);

    Task<Result<string>> GetUrlAsync(string objectKey, CancellationToken cancellationToken);
}
