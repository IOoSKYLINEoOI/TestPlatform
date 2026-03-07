using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Tags;

public interface ITagsReadRepository
{
    Task<TagResponse?> ReadTagByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<TagResponse>> ReadAllTagsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> GetExistingIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
}