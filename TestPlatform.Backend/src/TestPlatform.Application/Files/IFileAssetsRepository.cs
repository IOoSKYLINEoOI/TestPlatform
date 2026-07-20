using TestPlatform.Core.Files;

namespace TestPlatform.Application.Files;

public interface IFileAssetsRepository
{
    Task<FileAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(FileAsset fileAsset, CancellationToken cancellationToken);
}