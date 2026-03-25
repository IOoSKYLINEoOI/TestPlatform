using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ImageStorageService> _logger;

    public ImageStorageService(
        IOptions<ImageStorageOptions> options,
        ILogger<ImageStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<string>> SaveTempAsync(
    IFormFile file,
    CancellationToken cancellationToken)
{
    if (file.Length == 0)
    {
        _logger.LogWarning("Upload failed: file is empty");
        return Result.Failure<string>("file.empty");
    }

    var maxBytes = _options.MaxFileSizeMb * 1024 * 1024;
    if (file.Length > maxBytes)
    {
        _logger.LogWarning("Upload failed: file {FileName} exceeds max size of {MaxMb} MB", file.FileName, _options.MaxFileSizeMb);
        return Result.Failure<string>("file.too_large");
    }

    var extension = Path.GetExtension(file.FileName).ToLower();
    if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedExtensions.Contains(extension))
    {
        _logger.LogWarning("Upload failed: file {FileName} has invalid extension {Extension}", file.FileName, extension);
        return Result.Failure<string>("file.invalid_extension");
    }

    var fileName = $"{Guid.NewGuid():N}.webp";

    var tempPath = Path.Combine(
        _options.RootPath,
        _options.TempFolder,
        fileName);

    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);

        await using var inputStream = file.OpenReadStream();

        using var image = await Image.LoadAsync(inputStream, cancellationToken);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(_options.MaxWidth, _options.MaxHeight),
        }));

        await image.SaveAsync(
            tempPath,
            new WebpEncoder { Quality = _options.WebpQuality },
            cancellationToken);

        _logger.LogInformation("File {FileName} successfully saved to temp as {TempFileName}", file.FileName, fileName);
        return Result.Success(fileName);
    }
    catch (UnknownImageFormatException ex)
    {
        _logger.LogWarning(ex, "Invalid image format while uploading file {FileName}", file.FileName);
        return Result.Failure<string>("file.invalid_format");
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("Image upload cancelled for file {FileName}", file.FileName);
        return Result.Failure<string>("operation.cancelled");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error while saving temp image {FileName}", file.FileName);
        return Result.Failure<string>("file.save_error");
    }
}

    public Task<Result<string>> MoveToPermanent(
        string tempFileName,
        ImageFolder folder)
    {
        var sourcePath = Path.Combine(_options.RootPath, _options.TempFolder, tempFileName);

        if (!File.Exists(sourcePath))
        {
            _logger.LogWarning("Move failed: temp file not found {FileName}", tempFileName);
            return Task.FromResult(Result.Failure<string>("file.temp_not_found"));
        }

        var folderName = folder.ToString().ToLower();
        var destinationFolder = Path.Combine(_options.RootPath, _options.PermanentFolder, folderName);
        var destinationPath = Path.Combine(destinationFolder, tempFileName);

        try
        {
            Directory.CreateDirectory(destinationFolder);

            File.Move(sourcePath, destinationPath, overwrite: true);

            _logger.LogInformation("File {FileName} moved to permanent folder {Folder}", tempFileName, folder);
            return Task.FromResult(Result.Success(tempFileName));
        }
        catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32) // файл занят
        {
            _logger.LogWarning(ex, "File {FileName} is in use and cannot be moved", tempFileName);
            return Task.FromResult(Result.Failure<string>("file.in_use"));
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("MoveToPermanentAsync cancelled for file {FileName}", tempFileName);
            return Task.FromResult(Result.Failure<string>("operation.cancelled"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving file {FileName} to permanent storage", tempFileName);
            return Task.FromResult(Result.Failure<string>("file.move_error"));
        }
    }

    public Task<Result> DeleteTempAsync(string fileName)
    {
        var validation = ValidateFileName(fileName);
        if (!validation.IsSuccess)
            return Task.FromResult(Result.Failure(validation.Error));

        var path = Path.Combine(_options.RootPath, _options.TempFolder, fileName);

        try
        {
            if (File.Exists(path))
                File.Delete(path);

            return Task.FromResult(Result.Success());
        }
        catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32) // файл занят другим процессом
        {
            _logger.LogWarning(ex, "Temp file {FileName} is in use and cannot be deleted", fileName);
            return Task.FromResult(Result.Failure("file.in_use"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting temp file {FileName}", fileName);
            return Task.FromResult(Result.Failure("file.delete_error"));
        }
    }

    public Task<Result> DeletePermanentAsync(ImageFolder folder, string fileName)
    {
        var validation = ValidateFileName(fileName);
        if (!validation.IsSuccess)
            return Task.FromResult(Result.Failure(validation.Error));

        var path = Path.Combine(
            _options.RootPath,
            _options.PermanentFolder,
            folder.ToString().ToLower(),
            fileName);

        try
        {
            if (File.Exists(path))
                File.Delete(path);

            return Task.FromResult(Result.Success());
        }
        catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32) // файл занят другим процессом
        {
            _logger.LogWarning(ex, "Permanent file {FileName} in folder {Folder} is in use and cannot be deleted", fileName, folder);
            return Task.FromResult(Result.Failure("file.in_use"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting permanent file {FileName} in folder {Folder}", fileName, folder);
            return Task.FromResult(Result.Failure("file.delete_error"));
        }
    }

    public async Task<Result<Stream>> GetPermanentImageStreamAsync(
        ImageFolder folder,
        string fileName,
        CancellationToken cancellationToken)
    {
        var validation = ValidateFileName(fileName);
        if (!validation.IsSuccess)
            return Result.Failure<Stream>(validation.Error);

        var path = Path.Combine(
            _options.RootPath,
            _options.PermanentFolder,
            folder.ToString().ToLower(),
            fileName);

        if (!File.Exists(path))
            return Result.Failure<Stream>("file.not_found");

        try
        {
            var memory = new MemoryStream();

            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            await stream.CopyToAsync(memory, cancellationToken);

            memory.Position = 0;

            return Result.Success<Stream>(memory);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetPermanentImageStreamAsync cancelled for file {FileName} in folder {Folder}", fileName, folder);
            return Result.Failure<Stream>("operation.cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading permanent file {FileName} in folder {Folder}", fileName, folder);
            return Result.Failure<Stream>("file.read_error");
        }
    }

    public Result<string> GetPermanentImageUrl(ImageFolder folder, string fileName)
    {
        var validation = ValidateFileName(fileName);
        if (!validation.IsSuccess)
            return Result.Failure<string>(validation.Error);

        var url = $"/images/{_options.PermanentFolder}/{folder.ToString().ToLower()}/{fileName}";
        return Result.Success(url);
    }

    private static Result ValidateFileName(string fileName)
    {
        if (fileName.Contains(".."))
            return Result.Failure("file.invalid_name");

        if (!fileName.EndsWith(".webp"))
            return Result.Failure("file.invalid_format");

        return Result.Success();
    }
}
