using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Files;
using TestPlatform.Core.Files;

namespace TestPlatform.Infrastructure.Postgres.Files;

public class FileAssetsRepository(TestPlatformDbContext dbContext) : IFileAssetsRepository
{
    public Task<FileAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.FileAssets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(FileAsset fileAsset, CancellationToken cancellationToken)
        => await dbContext.FileAssets.AddAsync(fileAsset, cancellationToken);
}