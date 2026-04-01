using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Application.Questions.Features.GetAllQuestionsByTagsQuery;

public record GetAllQuestionsByTagsQuery(IReadOnlyList<Guid> TagIds, bool IncludeCorrectAnswer) : IQuery;

public class GetAllQuestionsByTagsHandler : IQueryHandler<IReadOnlyList<QuestionResponse>, GetAllQuestionsByTagsQuery>
{
    private readonly IQuestionsReadRepository _questionsReadRepository;
    private readonly ILogger<GetAllQuestionsByTagsHandler> _logger;

    public GetAllQuestionsByTagsHandler(IQuestionsReadRepository questionsReadRepository, ILogger<GetAllQuestionsByTagsHandler> logger)
    {
        _questionsReadRepository = questionsReadRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<QuestionResponse>>> Handle(
        GetAllQuestionsByTagsQuery query,
        CancellationToken cancellationToken)
    {
        var questions = await _questionsReadRepository
            .ReadAllQuestionsByTagsAsync(query.TagIds, query.IncludeCorrectAnswer, cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} Questions for tags {Tags}",
            questions.Count,
            string.Join(", ", query.TagIds));

        return Result.Success((IReadOnlyList<QuestionResponse>)questions);
    }
}