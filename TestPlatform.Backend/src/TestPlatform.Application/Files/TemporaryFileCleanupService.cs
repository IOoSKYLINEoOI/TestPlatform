using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Core.Files;

namespace TestPlatform.Application.Files;

public sealed class TemporaryFileCleanupService(
    IFileAssetsReadDbContext readDbContext,
    IFileAssetService fileAssetService,
    ILogger<TemporaryFileCleanupService> logger) : ITemporaryFileCleanupService
{
    public async Task<TemporaryFileCleanupResult> CleanupAsync(
        DateTime createdBefore,
        int batchSize,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        var fileIds = await readDbContext.ReadFileAssets
            .AsNoTracking()
            .Where(file => file.Status == FileAssetStatus.DeletionPending
                           || (file.Status == FileAssetStatus.Temporary
                               && file.CreatedAt < createdBefore))
            .OrderBy(file => file.CreatedAt)
            .Select(file => file.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var deleted = 0;
        foreach (var fileId in fileIds)
        {
            var result = await fileAssetService.DeleteAsync(
                fileId,
                Guid.Empty,
                canManageAll: true,
                cancellationToken);

            if (result.IsSuccess)
            {
                deleted++;
            }
            else
            {
                logger.LogWarning(
                    "Failed to delete temporary file {FileId}: {ErrorCode}.",
                    fileId,
                    result.Error);
            }
        }

        return new TemporaryFileCleanupResult(fileIds.Count, deleted, fileIds.Count - deleted);
    }
}
