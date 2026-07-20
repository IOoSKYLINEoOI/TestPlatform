using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using TestPlatform.Application.Files;

namespace TestPlatform.Infrastructure.Files.ImageProcessing;

public class ImageSharpImageProcessor : IImageProcessor
{
    private readonly ImageProcessingOptions _options;

    public ImageSharpImageProcessor(IOptions<ImageProcessingOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Result<ProcessedImage>> ProcessAsync(
        FileUploadRequest source,
        CancellationToken cancellationToken)
    {
        if (source.Length == 0)
            return Result.Failure<ProcessedImage>("file.empty");

        var maxBytes = _options.MaxFileSizeMb * 1024 * 1024;
        if (source.Length > maxBytes)
            return Result.Failure<ProcessedImage>("file.too_large");

        var extension = Path.GetExtension(source.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedExtensions.Contains(extension))
            return Result.Failure<ProcessedImage>("file.invalid_extension");

        await using var output = new MemoryStream();

        try
        {
            using var image = await Image.LoadAsync(source.Content, cancellationToken);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(_options.MaxWidth, _options.MaxHeight),
            }));

            await image.SaveAsync(
                output,
                new WebpEncoder { Quality = _options.WebpQuality },
                cancellationToken);
        }
        catch (UnknownImageFormatException)
        {
            return Result.Failure<ProcessedImage>("file.invalid_format");
        }

        return Result.Success(new ProcessedImage(
            new MemoryStream(output.ToArray()),
            "image/webp",
            ".webp"));
    }
}
