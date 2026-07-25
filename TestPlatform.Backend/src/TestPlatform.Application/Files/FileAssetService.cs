using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Files;

namespace TestPlatform.Application.Files;

public class FileAssetService : IFileAssetService
{
    private readonly IFileAssetsRepository _fileAssetsRepository;
    private readonly IObjectStorage _objectStorage;
    private readonly IImageProcessor _imageProcessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FileAssetService> _logger;

    public FileAssetService(
        IFileAssetsRepository fileAssetsRepository,
        IObjectStorage objectStorage,
        IImageProcessor imageProcessor,
        IUnitOfWork unitOfWork,
        ILogger<FileAssetService> logger)
    {
        _fileAssetsRepository = fileAssetsRepository;
        _objectStorage = objectStorage;
        _imageProcessor = imageProcessor;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FileAssetUploadResult>> UploadImageAsync(
        FileUploadRequest file,
        Guid uploadedByUserId,
        CancellationToken cancellationToken)
    {
        var processedImageResult = await _imageProcessor.ProcessAsync(file, cancellationToken);
        if (processedImageResult.IsFailure)
        {
            return Result.Failure<FileAssetUploadResult>(processedImageResult.Error);
        }

        await using var processedImage = processedImageResult.Value.Content;

        var fileId = Guid.NewGuid();
        var objectKey = $"images/{DateTime.UtcNow:yyyy/MM}/{fileId:N}.webp";

        var fileAssetResult = FileAsset.Create(
            fileId,
            objectKey,
            $"{fileId:N}{processedImageResult.Value.FileExtension}",
            processedImageResult.Value.ContentType,
            processedImage.Length,
            uploadedByUserId);

        if (fileAssetResult.IsFailure)
        {
            return Result.Failure<FileAssetUploadResult>(fileAssetResult.Error);
        }

        var putResult = await _objectStorage.PutAsync(
            objectKey,
            processedImage,
            processedImage.Length,
            processedImageResult.Value.ContentType,
            cancellationToken);

        if (putResult.IsFailure)
        {
            return Result.Failure<FileAssetUploadResult>(putResult.Error);
        }

        try
        {
            await _fileAssetsRepository.AddAsync(fileAssetResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to persist file asset {FileId}; compensating object storage upload.",
                fileId);
            await CompensateUploadAsync(objectKey);
            throw;
        }

        return Result.Success(new FileAssetUploadResult(fileAssetResult.Value.Id));
    }

    public async Task<Result> AttachAsync(Guid fileId, Guid userId, CancellationToken cancellationToken)
    {
        var fileAsset = await _fileAssetsRepository.GetByIdAsync(fileId, cancellationToken);
        if (fileAsset is null)
        {
            return Result.Failure("file.not_found");
        }

        var result = fileAsset.Attach(userId);
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid fileId,
        Guid userId,
        bool canManageAll,
        CancellationToken cancellationToken)
    {
        var fileAsset = await _fileAssetsRepository.GetByIdAsync(fileId, cancellationToken);
        if (fileAsset is null)
        {
            return Result.Success();
        }

        if (fileAsset.UploadedByUserId != userId && !canManageAll)
        {
            return Result.Failure("file.forbidden");
        }

        if (fileAsset.Status == FileAssetStatus.Deleted)
        {
            return Result.Success();
        }

        if (await _fileAssetsRepository.IsReferencedAsync(fileId, cancellationToken))
        {
            return Result.Failure("file.in_use");
        }

        if (fileAsset.Status != FileAssetStatus.DeletionPending)
        {
            fileAsset.RequestDeletion();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var deleteResult = await _objectStorage.DeleteAsync(
            fileAsset.ObjectKey,
            cancellationToken);
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        fileAsset.MarkDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ReleaseIfUnreferencedAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var fileAsset = await _fileAssetsRepository.GetByIdAsync(fileId, cancellationToken);
        if (fileAsset is null || fileAsset.Status is FileAssetStatus.Deleted or FileAssetStatus.DeletionPending)
        {
            return Result.Success();
        }

        if (await _fileAssetsRepository.IsReferencedAsync(fileId, cancellationToken))
        {
            return Result.Success();
        }

        fileAsset.RequestDeletion();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<Stream>> GetStreamAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var fileAsset = await _fileAssetsRepository.GetByIdAsync(fileId, cancellationToken);
        if (fileAsset is null
            || fileAsset.Status is FileAssetStatus.DeletionPending or FileAssetStatus.Deleted)
        {
            return Result.Failure<Stream>("file.not_found");
        }

        return await _objectStorage.GetAsync(fileAsset.ObjectKey, cancellationToken);
    }

    public async Task<Result<string>> GetUrlAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var fileAsset = await _fileAssetsRepository.GetByIdAsync(fileId, cancellationToken);
        if (fileAsset is null
            || fileAsset.Status is FileAssetStatus.DeletionPending or FileAssetStatus.Deleted)
        {
            return Result.Failure<string>("file.not_found");
        }

        return await _objectStorage.GetUrlAsync(fileAsset.ObjectKey, cancellationToken);
    }

    private async Task CompensateUploadAsync(string objectKey)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var result = await _objectStorage.DeleteAsync(objectKey, timeout.Token);
            if (result.IsFailure)
            {
                _logger.LogCritical(
                    "Failed to compensate object storage upload for {ObjectKey}: {ErrorCode}.",
                    objectKey,
                    result.Error);
            }
        }
        catch (Exception exception)
        {
            _logger.LogCritical(
                exception,
                "Compensation threw while deleting object {ObjectKey}.",
                objectKey);
        }
    }
}
