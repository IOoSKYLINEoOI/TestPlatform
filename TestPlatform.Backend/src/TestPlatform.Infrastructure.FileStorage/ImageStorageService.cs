using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Abstractions.Enums;

namespace TestPlatform.Infrastructure.FileStorage;

public class ImageStorageService : IImageStorageService
{
    private readonly ImageStorageOptions _options;

    public ImageStorageService(IOptions<ImageStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveTempAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            throw new ArgumentException("Empty file");

        var fileName = $"{Guid.NewGuid():N}.webp";

        var tempPath = Path.Combine(
            _options.RootPath,
            _options.TempFolder,
            fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);

        await using var inputStream = file.OpenReadStream();

        using var image = await Image.LoadAsync(inputStream, cancellationToken);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(_options.MaxWidth, _options.MaxHeight),
        }));

        await image.SaveAsync(tempPath, new WebpEncoder { Quality = 75 }, cancellationToken);

        return fileName;
    }

    public async Task<string> MoveToPermanentAsync(
        string tempFileName,
        ImageFolder folder,
        CancellationToken cancellationToken)
    {
        ValidateFileName(tempFileName);

        var sourcePath = Path.Combine(
            _options.RootPath,
            _options.TempFolder,
            tempFileName);

        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Temp file not found");

        var folderName = folder.ToString().ToLower();

        var destinationFolder = Path.Combine(
            _options.RootPath,
            _options.PermanentFolder,
            folderName);

        Directory.CreateDirectory(destinationFolder);

        var destinationPath = Path.Combine(destinationFolder, tempFileName);

        await using var sourceStream = new FileStream(
            sourcePath,
            FileMode.Open,
            FileAccess.Read);

        await using var destinationStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write);

        await sourceStream.CopyToAsync(destinationStream, cancellationToken);

        File.Delete(sourcePath);

        return tempFileName;
    }

    public Task DeleteTempAsync(string fileName)
    {
        ValidateFileName(fileName);

        var path = Path.Combine(
            _options.RootPath,
            _options.TempFolder,
            fileName);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public Task DeletePermanentAsync(ImageFolder folder, string fileName)
    {
        ValidateFileName(fileName);

        var path = Path.Combine(
            _options.RootPath,
            _options.PermanentFolder,
            folder.ToString().ToLower(),
            fileName);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public async Task<Stream> GetPermanentImageStreamAsync(
        ImageFolder folder,
        string fileName,
        CancellationToken cancellationToken)
    {
        ValidateFileName(fileName);

        var path = Path.Combine(
            _options.RootPath,
            _options.PermanentFolder,
            folder.ToString().ToLower(),
            fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException();

        var memory = new MemoryStream();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        await stream.CopyToAsync(memory, cancellationToken);

        memory.Position = 0;

        return memory;
    }

    public string GetPermanentImageUrl(ImageFolder folder, string fileName)
    {
        ValidateFileName(fileName);

        return $"/images/{_options.PermanentFolder}/{folder.ToString().ToLower()}/{fileName}";
    }

    private static void ValidateFileName(string fileName)
    {
        if (fileName.Contains(".."))
            throw new ArgumentException("Invalid file name");

        if (!fileName.EndsWith(".webp"))
            throw new ArgumentException("Invalid image format");
    }
}
