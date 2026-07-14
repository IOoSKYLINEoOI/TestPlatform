using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Questions.Extensions;
using TestPlatform.Contracts.Questions.DTOs.Preview;

namespace TestPlatform.Application.Questions.Features.GetAllQuestionsByTagsQuery;

public record GetAllQuestionsByTagsQuery(IReadOnlyList<Guid> TagIds) : IQuery;

public class GetAllQuestionsByTagsHandler(IQuestionsReadDbContext questionsReadDbContext)
    : IQueryHandler<GetAllQuestionsByTagsQuery, IReadOnlyList<QuestionPreviewResponse>>
{
    public async Task<Result<IReadOnlyList<QuestionPreviewResponse>>> Handle(
        GetAllQuestionsByTagsQuery query,
        CancellationToken cancellationToken)
    {
        var tagIds = query.TagIds.Distinct().ToList();

        var questions = await questionsReadDbContext.ReadQuestions
            .AsNoTracking()
            .Where(q => tagIds.All(id => q.Tags.Any(t => t.Id == id)))
            .Include(q => q.Tags)
            .ToListAsync(cancellationToken);

        var result = questions
            .Select(q => q.ToPreviewResponse())
            .ToList();

        return Result.Success<IReadOnlyList<QuestionPreviewResponse>>(result);
    }
}
