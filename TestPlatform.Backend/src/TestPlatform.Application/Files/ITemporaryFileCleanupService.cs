namespace TestPlatform.Application.Files;

public interface ITemporaryFileCleanupService
{
    Task<TemporaryFileCleanupResult> CleanupAsync(
        DateTime createdBefore,
        int batchSize,
        CancellationToken cancellationToken);
}

public sealed record TemporaryFileCleanupResult(int Found, int Deleted, int Failed);
