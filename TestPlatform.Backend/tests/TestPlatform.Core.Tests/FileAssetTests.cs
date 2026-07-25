using TestPlatform.Core.Files;
using Xunit;

namespace TestPlatform.Core.Tests;

public sealed class FileAssetTests
{
    [Fact]
    public void Attach_RepeatedByOwner_IsIdempotent()
    {
        var ownerId = Guid.NewGuid();
        var file = CreateFile(ownerId);
        file.Attach(ownerId);
        var attachedAt = file.AttachedAt;

        var result = file.Attach(ownerId);

        Assert.True(result.IsSuccess);
        Assert.Equal(attachedAt, file.AttachedAt);
    }

    [Fact]
    public void Attach_WhenDeletionPending_IsRejected()
    {
        var ownerId = Guid.NewGuid();
        var file = CreateFile(ownerId);
        file.RequestDeletion();

        var result = file.Attach(ownerId);

        Assert.True(result.IsFailure);
        Assert.Equal("file.deleted", result.Error);
    }

    private static FileAsset CreateFile(Guid ownerId)
        => FileAsset.Create(
            Guid.NewGuid(),
            $"images/{Guid.NewGuid():N}.webp",
            "image.webp",
            "image/webp",
            100,
            ownerId).Value;
}
