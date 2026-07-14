using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Tags.Features.GetAllTagsQuery;

public record GetAllTagsQuery : IQuery;

public class GetAllTagsHandler(ITagsReadDbContext tagsDbContext)
    : IQueryHandler<GetAllTagsQuery, IReadOnlyList<TagResponse>>
{
    public async Task<Result<IReadOnlyList<TagResponse>>> Handle(GetAllTagsQuery query, CancellationToken cancellationToken)
    {
        var response = await tagsDbContext.ReadTags
            .Select(x => new TagResponse(
                Id: x.Id,
                Name: x.Name,
                Description: x.Description))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TagResponse>>(response);
    }
}
