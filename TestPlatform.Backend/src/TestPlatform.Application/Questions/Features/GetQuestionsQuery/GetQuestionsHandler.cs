using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Questions.Extensions;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Application.Questions.Features.GetQuestionsQuery;

public record GetQuestionsQuery(
    IReadOnlyList<Guid> TagIds,
    QuestionStatus? Status,
    int Page,
    int PageSize) : IQuery;

public sealed class GetQuestionsHandler(IQuestionsReadDbContext questionsReadDbContext)
    : IQueryHandler<GetQuestionsQuery, QuestionPageResponse>
{
    public async Task<Result<QuestionPageResponse>> Handle(
        GetQuestionsQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var tagIds = query.TagIds.Distinct().ToList();

        var questions = questionsReadDbContext.ReadQuestions
            .AsNoTracking()
            .AsQueryable();

        if (query.Status.HasValue)
        {
            questions = questions.Where(question => question.Status == query.Status.Value);
        }

        if (tagIds.Count != 0)
        {
            questions = questions.Where(question => tagIds.All(id => question.Tags.Any(tag => tag.Id == id)));
        }

        var totalCount = await questions.CountAsync(cancellationToken);
        var items = await questions
            .Include(question => question.Tags)
            .OrderByDescending(question => question.UpdatedAt)
            .ThenBy(question => question.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new QuestionPageResponse(
            items.Select(question => question.ToPreviewResponse()).ToList(),
            page,
            pageSize,
            totalCount));
    }
}
