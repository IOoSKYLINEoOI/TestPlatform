using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Questions.Validators;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Application.Questions.Features.GetByIdQuestionQuery;

public record GetByIdQuestionQuery(Guid Id, bool IncludeCorrectAnswer) : IQuery;

public class GetByIdQuestionHandler : IQueryHandler<QuestionResponse, GetByIdQuestionQuery>
{
    private readonly IQuestionsReadRepository _questionsReadRepository;
    private readonly ILogger<GetByIdQuestionHandler> _logger;

    public GetByIdQuestionHandler(IQuestionsReadRepository questionsReadRepository, ILogger<GetByIdQuestionHandler> logger)
    {
        _questionsReadRepository = questionsReadRepository;
        _logger = logger;
    }

    public async Task<QuestionResponse?> Handle(GetByIdQuestionQuery query, CancellationToken cancellationToken)
    {
        var question =
            await _questionsReadRepository.ReadQuestionByIdAsync(query.Id, query.IncludeCorrectAnswer, cancellationToken);

        if (question == null)
            _logger.LogWarning("Question with id {Id} not found", query.Id);
        else
            _logger.LogInformation("Get Question with id {Id}", query.Id);

        return question;
    }
}