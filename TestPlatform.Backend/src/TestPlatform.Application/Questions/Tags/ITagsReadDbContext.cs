using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions.Tags;

public interface ITagsReadDbContext
{
    IQueryable<Tag> ReadTags { get; }
}
