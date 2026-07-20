using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Questions.Tags.Features.GetTagsQuery;

public record GetTagsQuery(string? Search, int Page, int PageSize) : IQuery;

public class GetTagsHandler(ITagsReadDbContext tagsDbContext)
    : IQueryHandler<GetTagsQuery, TagPageResponse>
{
    public async Task<Result<TagPageResponse>> Handle(GetTagsQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var search = query.Search?.Trim();

        var tags = tagsDbContext.ReadTags;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToUpperInvariant();
            tags = tags.Where(tag => tag.Name.ToUpper().Contains(normalizedSearch));
        }

        var totalCount = await tags.CountAsync(cancellationToken);
        var items = await tags
            .OrderBy(tag => tag.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(tag => new TagResponse(tag.Id, tag.Name, tag.Description))
            .ToListAsync(cancellationToken);

        return Result.Success(new TagPageResponse(items, page, pageSize, totalCount));
    }
}
