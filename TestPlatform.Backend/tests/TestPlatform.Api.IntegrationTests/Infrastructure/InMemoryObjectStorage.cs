using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using TestPlatform.Application.Files;

namespace TestPlatform.Api.IntegrationTests.Infrastructure;

public sealed class InMemoryObjectStorage : IObjectStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _objects = new();
    private int _failNextDelete;
    private int _deleteCallCount;

    public int DeleteCallCount => Volatile.Read(ref _deleteCallCount);

    public void FailNextDelete() => Interlocked.Exchange(ref _failNextDelete, 1);

    public async Task<Result> PutAsync(
        string objectKey,
        Stream stream,
        long sizeBytes,
        string contentType,
        CancellationToken cancellationToken)
    {
        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        _objects[objectKey] = buffer.ToArray();
        return Result.Success();
    }

    public Task<Result<Stream>> GetAsync(
        string objectKey,
        CancellationToken cancellationToken) =>
        Task.FromResult(_objects.TryGetValue(objectKey, out var content)
            ? Result.Success<Stream>(new MemoryStream(content, writable: false))
            : Result.Failure<Stream>("file.not_found"));

    public Task<Result> DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _deleteCallCount);
        if (Interlocked.Exchange(ref _failNextDelete, 0) == 1)
        {
            return Task.FromResult(Result.Failure("file.delete_error"));
        }

        _objects.TryRemove(objectKey, out _);
        return Task.FromResult(Result.Success());
    }

    public Task<Result<string>> GetUrlAsync(
        string objectKey,
        CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success($"https://storage.test/{objectKey}"));
}
