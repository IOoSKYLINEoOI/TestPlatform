using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Tags;

public interface ITagsRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Tag tag, CancellationToken cancellationToken);

    void Delete(Tag tag);
}