using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions;
using TestPlatform.Application.Questions.Extensions;
using TestPlatform.Contracts.Questions.DTOs.Preview;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Questions.Tags.Features.GetTagQuestionsQuery;

public record GetTagQuestionsQuery(Guid TagId, int Page, int PageSize) : IQuery;

public class GetTagQuestionsHandler(
    ITagsReadDbContext tagsReadDbContext,
    IQuestionsReadDbContext questionsReadDbContext)
    : IQueryHandler<GetTagQuestionsQuery, TagPageQuestionsResponse>
{
    public async Task<Result<TagPageQuestionsResponse>> Handle(
        GetTagQuestionsQuery query,
        CancellationToken cancellationToken)
    {
        var exists = await tagsReadDbContext.ReadTags
            .AnyAsync(tag => tag.Id == query.TagId, cancellationToken);

        if (!exists)
            return Result.Failure<TagPageQuestionsResponse>(ErrorCodes.TagNotFound);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var questions = questionsReadDbContext.ReadQuestions
            .Where(question => question.Tags.Any(tag => tag.Id == query.TagId));

        var totalCount = await questions.CountAsync(cancellationToken);
        var items = await questions
            .Include(question => question.Tags)
            .OrderBy(question => question.Text)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new TagPageQuestionsResponse(
            items.Select(question => question.ToPreviewResponse()).ToList(),
            page,
            pageSize,
            totalCount));
    }
}
