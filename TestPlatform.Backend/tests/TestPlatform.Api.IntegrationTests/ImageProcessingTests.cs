using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using TestPlatform.Application.Files;
using TestPlatform.Infrastructure.Files.ImageProcessing;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class ImageProcessingTests
{
    [Fact]
    public async Task ProcessAsync_ValidPng_ResizesAndConvertsToWebp()
    {
        await using var source = new MemoryStream();
        using (var image = new Image<Rgba32>(200, 100))
        {
            await image.SaveAsPngAsync(source);
        }

        source.Position = 0;
        var processor = CreateProcessor(maxWidth: 50, maxHeight: 50);

        var result = await processor.ProcessAsync(
            new FileUploadRequest("photo.png", "image/png", source.Length, source),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("image/webp", result.Value.ContentType);
        Assert.Equal(".webp", result.Value.FileExtension);
        using var processed = await Image.LoadAsync(result.Value.Content);
        Assert.Equal(50, processed.Width);
        Assert.Equal(25, processed.Height);
        Assert.IsType<WebpFormat>(processed.Metadata.DecodedImageFormat);
    }

    [Fact]
    public async Task ProcessAsync_CorruptedImage_ReturnsInvalidFormat()
    {
        await using var source = new MemoryStream([1, 2, 3, 4]);
        var processor = CreateProcessor();

        var result = await processor.ProcessAsync(
            new FileUploadRequest("photo.png", "image/png", source.Length, source),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("file.invalid_format", result.Error);
    }

    [Fact]
    public async Task ProcessAsync_DisallowedExtension_ReturnsInvalidExtension()
    {
        await using var source = new MemoryStream([1]);
        var processor = CreateProcessor();

        var result = await processor.ProcessAsync(
            new FileUploadRequest("photo.svg", "image/svg+xml", source.Length, source),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("file.invalid_extension", result.Error);
    }

    private static ImageSharpImageProcessor CreateProcessor(
        int maxWidth = 1024,
        int maxHeight = 1024)
    {
        return new ImageSharpImageProcessor(Options.Create(new ImageProcessingOptions
        {
            MaxWidth = maxWidth,
            MaxHeight = maxHeight,
        }));
    }
}
