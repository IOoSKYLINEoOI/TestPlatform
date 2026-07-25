using TestPlatform.Core.Files;

namespace TestPlatform.Application.Files;

public interface IFileAssetsRepository
{
    Task<FileAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<FileAsset>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    Task AddAsync(FileAsset fileAsset, CancellationToken cancellationToken);

    Task<bool> IsReferencedAsync(Guid fileId, CancellationToken cancellationToken);
}
