using CSharpFunctionalExtensions;

namespace TestPlatform.Application.Files;

public interface IFileAssetService
{
    Task<Result<FileAssetUploadResult>> UploadImageAsync(
        FileUploadRequest file,
        Guid uploadedByUserId,
        CancellationToken cancellationToken);

    Task<Result> AttachAsync(Guid fileId, Guid userId, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid fileId, Guid userId, CancellationToken cancellationToken);

    Task<Result<Stream>> GetStreamAsync(Guid fileId, CancellationToken cancellationToken);

    Task<Result<string>> GetUrlAsync(Guid fileId, CancellationToken cancellationToken);
}

public record FileAssetUploadResult(Guid FileId, string Url);

public record FileUploadRequest(
    string FileName,
    string ContentType,
    long Length,
    Stream Content);
