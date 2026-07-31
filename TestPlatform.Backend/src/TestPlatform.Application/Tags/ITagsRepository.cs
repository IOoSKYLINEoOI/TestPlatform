using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Tags;

public interface ITagsRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Tag>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(Guid? excludedTagId, string name, CancellationToken cancellationToken);

    Task<int> GetUsageCountAsync(Guid tagId, CancellationToken cancellationToken);

    Task<int> MergeAsync(Tag sourceTag, Tag targetTag, CancellationToken cancellationToken);

    Task AddAsync(Tag tag, CancellationToken cancellationToken);

    void Delete(Tag tag);
}
