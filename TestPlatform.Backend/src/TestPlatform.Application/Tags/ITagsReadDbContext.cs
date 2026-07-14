using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Tags;

public interface ITagsReadDbContext
{
    IQueryable<Tag> ReadTags { get; }
}