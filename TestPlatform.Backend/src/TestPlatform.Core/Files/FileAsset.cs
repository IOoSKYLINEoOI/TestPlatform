using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Files;

public class FileAsset
{
    private FileAsset() { }

    private FileAsset(
        Guid id,
        string objectKey,
        string fileName,
        string contentType,
        long sizeBytes,
        Guid uploadedByUserId)
    {
        Id = id;
        ObjectKey = objectKey;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        UploadedByUserId = uploadedByUserId;
        Status = FileAssetStatus.Temporary;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public string ObjectKey { get; private set; } = null!;

    public string FileName { get; private set; } = null!;

    public string ContentType { get; private set; } = null!;

    public long SizeBytes { get; private set; }

    public Guid UploadedByUserId { get; private set; }

    public FileAssetStatus Status { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? AttachedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public static Result<FileAsset> Create(
        Guid id,
        string objectKey,
        string fileName,
        string contentType,
        long sizeBytes,
        Guid uploadedByUserId)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            return Result.Failure<FileAsset>("file.object_key_required");

        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure<FileAsset>("file.name_required");

        if (string.IsNullOrWhiteSpace(contentType))
            return Result.Failure<FileAsset>("file.content_type_required");

        if (sizeBytes <= 0)
            return Result.Failure<FileAsset>("file.empty");

        if (uploadedByUserId == Guid.Empty)
            return Result.Failure<FileAsset>("file.uploader_required");

        if (id == Guid.Empty)
            return Result.Failure<FileAsset>("file.id_required");

        return Result.Success(new FileAsset(
            id,
            objectKey,
            fileName,
            contentType,
            sizeBytes,
            uploadedByUserId));
    }

    public Result Attach(Guid userId)
    {
        if (Status == FileAssetStatus.Deleted)
            return Result.Failure("file.deleted");

        if (UploadedByUserId != userId)
            return Result.Failure("file.forbidden");

        Status = FileAssetStatus.Attached;
        AttachedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result MarkDeleted()
    {
        if (Status == FileAssetStatus.Deleted)
            return Result.Success();

        Status = FileAssetStatus.Deleted;
        DeletedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
