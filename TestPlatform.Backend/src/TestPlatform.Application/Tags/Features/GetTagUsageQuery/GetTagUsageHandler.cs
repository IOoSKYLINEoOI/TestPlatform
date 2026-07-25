using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Tags.Features.GetTagUsageQuery;

public record GetTagUsageQuery(Guid TagId) : IQuery;

public class GetTagUsageHandler(
    ITagsReadDbContext tagsReadDbContext,
    IQuestionsReadDbContext questionsReadDbContext)
    : IQueryHandler<GetTagUsageQuery, TagUsageResponse>
{
    public async Task<Result<TagUsageResponse>> Handle(GetTagUsageQuery query, CancellationToken cancellationToken)
    {
        var exists = await tagsReadDbContext.ReadTags
            .AnyAsync(tag => tag.Id == query.TagId, cancellationToken);

        if (!exists)
        {
            return Result.Failure<TagUsageResponse>(ErrorCodes.TagNotFound);
        }

        var questionCount = await questionsReadDbContext.ReadQuestions
            .CountAsync(question => question.Tags.Any(tag => tag.Id == query.TagId), cancellationToken);

        return Result.Success(new TagUsageResponse(query.TagId, questionCount));
    }
}
