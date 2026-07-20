using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Questions.Tags.Features.GetByIdTagQuery;

public record GetByIdTagQuery(Guid Id) : IQuery;

public class GetByIdTagHandler(ITagsReadDbContext tagsDbContext) : IQueryHandler<GetByIdTagQuery, TagResponse>
{
    public async Task<Result<TagResponse>> Handle(GetByIdTagQuery query, CancellationToken cancellationToken)
    {
        var response = await tagsDbContext.ReadTags
            .Where(x => x.Id == query.Id)
            .Select(x => new TagResponse(x.Id, x.Name, x.Description))
            .FirstOrDefaultAsync(cancellationToken);

        return response is null
            ? Result.Failure<TagResponse>(ErrorCodes.TagNotFound)
            : Result.Success(response);
    }
}
