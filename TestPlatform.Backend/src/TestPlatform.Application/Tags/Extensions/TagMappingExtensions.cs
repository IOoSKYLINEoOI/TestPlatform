using TestPlatform.Contracts.Tags.DTOs;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Tags.Extensions;

public static class TagMappingExtensions
{
    public static IReadOnlyList<TagResponse> ToResponses(this IEnumerable<Tag> tags)
        => tags.Select(t => new TagResponse(t.Id, t.Name, t.Description)).ToList();
}