using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using TestPlatform.Api.IntegrationTests.Infrastructure;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Files;
using TestPlatform.Core.Files;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class FileAssetServiceTests
{
    [Fact]
    public async Task UploadImageAsync_DatabaseFailure_RemovesUploadedObject()
    {
        var storage = new TrackingObjectStorage();
        var service = CreateService(storage, new FailingUnitOfWork());
        await using var content = new MemoryStream([1, 2, 3]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadImageAsync(
                new FileUploadRequest("image.png", "image/png", content.Length, content),
                Guid.NewGuid(),
                CancellationToken.None));

        Assert.Equal(0, storage.ObjectCount);
        Assert.Equal(1, storage.DeleteCallCount);
        Assert.Equal(0, storage.GetUrlCallCount);
    }

    [Fact]
    public async Task UploadImageAsync_Success_DoesNotDependOnPresignedUrl()
    {
        var storage = new TrackingObjectStorage();
        var service = CreateService(storage, new SuccessfulUnitOfWork());
        await using var content = new MemoryStream([1, 2, 3]);

        var result = await service.UploadImageAsync(
            new FileUploadRequest("image.png", "image/png", content.Length, content),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, storage.ObjectCount);
        Assert.Equal(0, storage.GetUrlCallCount);
    }

    private static FileAssetService CreateService(
        TrackingObjectStorage storage,
        IUnitOfWork unitOfWork)
        => new(
            new TrackingFileAssetsRepository(),
            storage,
            new PassThroughImageProcessor(),
            unitOfWork,
            NullLogger<FileAssetService>.Instance);

    private sealed class TrackingFileAssetsRepository : IFileAssetsRepository
    {
        public Task<FileAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult<FileAsset?>(null);

        public Task<IReadOnlyList<FileAsset>> GetByIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<FileAsset>>([]);

        public Task AddAsync(FileAsset fileAsset, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<bool> IsReferencedAsync(Guid fileId, CancellationToken cancellationToken)
            => Task.FromResult(false);
    }

    private sealed class SuccessfulUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class FailingUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken ct)
            => Task.FromException(new InvalidOperationException("Expected database failure."));
    }

    private sealed class TrackingObjectStorage : IObjectStorage
    {
        private readonly HashSet<string> _objects = [];

        public int ObjectCount => _objects.Count;

        public int DeleteCallCount { get; private set; }

        public int GetUrlCallCount { get; private set; }

        public Task<Result> PutAsync(
            string objectKey,
            Stream stream,
            long sizeBytes,
            string contentType,
            CancellationToken cancellationToken)
        {
            _objects.Add(objectKey);
            return Task.FromResult(Result.Success());
        }

        public Task<Result<Stream>> GetAsync(
            string objectKey,
            CancellationToken cancellationToken)
            => Task.FromResult(Result.Failure<Stream>("file.not_found"));

        public Task<Result> DeleteAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            DeleteCallCount++;
            _objects.Remove(objectKey);
            return Task.FromResult(Result.Success());
        }

        public Task<Result<string>> GetUrlAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            GetUrlCallCount++;
            throw new InvalidOperationException("Presigned URL must not be requested during upload.");
        }
    }
}
