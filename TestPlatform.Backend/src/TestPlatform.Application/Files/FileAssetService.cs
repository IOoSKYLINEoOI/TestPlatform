using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Files;

namespace TestPlatform.Application.Files;

public class FileAssetService : IFileAssetService
{
    private readonly IFileAssetsRepository _fileAssetsRepository;
    private readonly IObjectStorage _objectStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FileUploadOptions _imageOptions;

    public FileAssetService(
        IFileAssetsRepository fileAssetsRepository,
        IObjectStorage objectStorage,
        IUnitOfWork unitOfWork,
        IOptions<FileUploadOptions> imageOptions)
    {
        _fileAssetsRepository = fileAssetsRepository;
        _objectStorage = objectStorage;
        _unitOfWork = unitOfWork;
        _imageOptions = imageOptions.Value;
    }

    public async Task<Result<FileAssetUploadResult>> UploadImageAsync(
        IFormFile file,
        Guid uploadedByUserId,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return Result.Failure<FileAssetUploadResult>("file.empty");

        var maxBytes = _imageOptions.MaxFileSizeMb * 1024 * 1024;
        if (file.Length > maxBytes)
            return Result.Failure<FileAssetUploadResult>("file.too_large");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !_imageOptions.AllowedExtensions.Contains(extension))
            return Result.Failure<FileAssetUploadResult>("file.invalid_extension");

        var fileId = Guid.NewGuid();
        var objectKey = $"images/{DateTime.UtcNow:yyyy/MM}/{fileId:N}.webp";

        await using var inputStream = file.OpenReadStream();
        await using var outputStream = new MemoryStream();

        try
        {
            using var image = await Image.LoadAsync(inputStream, cancellationToken);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(_imageOptions.MaxWidth, _imageOptions.MaxHeight),
            }));

            await image.SaveAsync(
                outputStream,
                new WebpEncoder { Quality = _imageOptions.WebpQuality },
                cancellationToken);
        }
        catch (UnknownImageFormatException)
        {
            return Result.Failure<FileAssetUploadResult>("file.invalid_format");
        }

        outputStream.Position = 0;

        var putResult = await _objectStorage.PutAsync(
            objectKey,
            outputStream,
            outputStream.Length,
            "image/webp",
            cancellationToken);

        if (putResult.IsFailure)
            return Result.Failure<FileAssetUploadResult>(putResult.Error);

        var fileAssetResult = FileAsset.Create(
            fileId,
            objectKey,
            $"{fileId:N}.webp",
            "image/webp",
            outputStream.Length,
            uploadedByUserId);

        if (fileAssetResult.IsFailure)
            return Result.Failure<FileAssetUploadResult>(fileAssetResult.Error);

        await _fileAssetsRepository.AddAsync(fileAssetResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var urlResult = await _objectStorage.GetUrlAsync(
            fileAssetResult.Value.ObjectKey,
            cancellationToken);

        if (urlResult.IsFailure)
            return Result.Failure<FileAssetUploadResult>(urlResult.Error);

        return Result.Success(new FileAssetUploadResult(fileAssetResult.Value.Id, urlResult.Value));
    }

    public async Task<Result> AttachAsync(Guid fileId, Guid userId, CancellationToken cancellationToken)
    {
        var fileAsset = await _fileAssetsRepository.GetByIdAsync(fileId, cancellationToken);
        if (fileAsset is null)
            return Result.Failure("file.not_found");

        var result = fileAsset.Attach(userId);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid fileId, Guid userId, CancellationToken cancellationToken)
    {
        var fileAsset = await _fileAssetsRepository.GetByIdAsync(fileId, cancellationToken);
        if (fileAsset is null)
            return Result.Success();

        if (fileAsset.UploadedByUserId != userId)
            return Result.Failure("file.forbidden");

        fileAsset.MarkDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _objectStorage.DeleteAsync(fileAsset.ObjectKey, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<Stream>> GetStreamAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var fileAsset = await _fileAssetsRepository.GetByIdAsync(fileId, cancellationToken);
        if (fileAsset is null || fileAsset.Status == FileAssetStatus.Deleted)
            return Result.Failure<Stream>("file.not_found");

        return await _objectStorage.GetAsync(fileAsset.ObjectKey, cancellationToken);
    }

    public async Task<Result<string>> GetUrlAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var fileAsset = await _fileAssetsRepository.GetByIdAsync(fileId, cancellationToken);
        if (fileAsset is null || fileAsset.Status == FileAssetStatus.Deleted)
            return Result.Failure<string>("file.not_found");

        return await _objectStorage.GetUrlAsync(fileAsset.ObjectKey, cancellationToken);
    }
}
