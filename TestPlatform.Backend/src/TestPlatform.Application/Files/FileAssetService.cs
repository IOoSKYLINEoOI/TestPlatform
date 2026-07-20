using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Files;

namespace TestPlatform.Application.Files;

public class FileAssetService : IFileAssetService
{
    private readonly IFileAssetsRepository _fileAssetsRepository;
    private readonly IObjectStorage _objectStorage;
    private readonly IImageProcessor _imageProcessor;
    private readonly IUnitOfWork _unitOfWork;

    public FileAssetService(
        IFileAssetsRepository fileAssetsRepository,
        IObjectStorage objectStorage,
        IImageProcessor imageProcessor,
        IUnitOfWork unitOfWork)
    {
        _fileAssetsRepository = fileAssetsRepository;
        _objectStorage = objectStorage;
        _imageProcessor = imageProcessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FileAssetUploadResult>> UploadImageAsync(
        FileUploadRequest file,
        Guid uploadedByUserId,
        CancellationToken cancellationToken)
    {
        var processedImageResult = await _imageProcessor.ProcessAsync(file, cancellationToken);
        if (processedImageResult.IsFailure)
            return Result.Failure<FileAssetUploadResult>(processedImageResult.Error);

        await using var processedImage = processedImageResult.Value.Content;

        var fileId = Guid.NewGuid();
        var objectKey = $"images/{DateTime.UtcNow:yyyy/MM}/{fileId:N}.webp";

        var putResult = await _objectStorage.PutAsync(
            objectKey,
            processedImage,
            processedImage.Length,
            processedImageResult.Value.ContentType,
            cancellationToken);

        if (putResult.IsFailure)
            return Result.Failure<FileAssetUploadResult>(putResult.Error);

        var fileAssetResult = FileAsset.Create(
            fileId,
            objectKey,
            $"{fileId:N}{processedImageResult.Value.FileExtension}",
            processedImageResult.Value.ContentType,
            processedImage.Length,
            uploadedByUserId);

        if (fileAssetResult.IsFailure)
            return Result.Failure<FileAssetUploadResult>(fileAssetResult.Error);

        await _fileAssetsRepository.AddAsync(fileAssetResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var urlResult = await _objectStorage.GetUrlAsync(
            fileAssetResult.Value.ObjectKey,
            cancellationToken);

        return urlResult.IsSuccess
            ? Result.Success(new FileAssetUploadResult(fileAssetResult.Value.Id, urlResult.Value))
            : Result.Failure<FileAssetUploadResult>(urlResult.Error);
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

        return await _objectStorage.DeleteAsync(fileAsset.ObjectKey, cancellationToken);
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
