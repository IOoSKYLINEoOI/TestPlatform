using CSharpFunctionalExtensions;
using TestPlatform.Core.Tags;

namespace TestPlatform.Application.Tags;

public interface ITagsRepository
{
    Task<Result<Guid>> AddAsync(Tag tag, CancellationToken cancellationToken);

    Task<Result> UpdateAsync(Guid id, string name, string description, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}