using CSharpFunctionalExtensions;
using TestPlatform.Application.Files;

namespace TestPlatform.Api.IntegrationTests.Infrastructure;

public sealed class PassThroughImageProcessor : IImageProcessor
{
    public async Task<Result<ProcessedImage>> ProcessAsync(
        FileUploadRequest source,
        CancellationToken cancellationToken)
    {
        var content = new MemoryStream();
        await source.Content.CopyToAsync(content, cancellationToken);
        content.Position = 0;
        return Result.Success(new ProcessedImage(content, "image/webp", ".webp"));
    }
}
