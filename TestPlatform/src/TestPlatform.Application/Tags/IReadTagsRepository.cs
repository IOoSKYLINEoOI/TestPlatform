using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Tags;

public interface IReadTagsRepository
{
    Task<TagResponse?> ReadTagByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<TagResponse>> ReadAllTagsAsync(CancellationToken cancellationToken);
}