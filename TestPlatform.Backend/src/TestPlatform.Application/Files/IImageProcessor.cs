using CSharpFunctionalExtensions;

namespace TestPlatform.Application.Files;

public interface IImageProcessor
{
    Task<Result<ProcessedImage>> ProcessAsync(
        FileUploadRequest source,
        CancellationToken cancellationToken);
}

public record ProcessedImage(Stream Content, string ContentType, string FileExtension);
