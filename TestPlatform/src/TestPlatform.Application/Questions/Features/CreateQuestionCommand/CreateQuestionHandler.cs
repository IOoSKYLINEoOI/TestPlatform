using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions.Features.CreateQuestionCommand;

public record CreateQuestionCommand(QuestionRequest Request) : ICommand;

public class CreateQuestionHandler
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly ILogger<CreateQuestionHandler> _logger;

    public CreateQuestionHandler(IQuestionsRepository questionsRepository, ILogger<CreateQuestionHandler> logger)
    {
        _questionsRepository = questionsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateQuestionCommand command, CancellationToken cancellationToken)
    {
        var questionResult = Question.Create(
            command.Request.Text,
            (QuestionType)command.Request.QuestionTypeId,
            command.Request.Points ?? 1,
            command.Request.ImageUrl);

        if (questionResult.IsFailure)
            return Result.Failure<Guid>(questionResult.Error);

        var question = questionResult.Value;

        // SingleChoice Question
        if (question.QuestionType == QuestionType.SingleChoice)
        {
            if (command.Request.AnswerOptions.Count(x => x.IsCorrect) != 1)
            {
                _logger.LogError("Failed to create Question. Invalid count of correct answers.");

                return Result.Failure<Guid>("SingleChoice question must have exactly one correct answer.");
            }
        }

        // MultiChoice Question
        if (question.QuestionType == QuestionType.MultipleChoice)
        {
            if (command.Request.AnswerOptions.Count(x => x.IsCorrect) < 1)
            {
                _logger.LogError("Failed to create Question. No correct answer provided.");
                return Result.Failure<Guid>("MultipleChoice question must have at least one correct answer.");
            }
        }

        var answerResults = command.Request.AnswerOptions
            .Select(o => AnswerOption.Create(o.Text, o.IsCorrect, o.ImageUrl))
            .ToList();

        var combined = Result.Combine(answerResults);
        if (combined.IsFailure)
        {
            _logger.LogError("Failed to create answer options: {Error}", combined.Error);
            return Result.Failure<Guid>(combined.Error);
        }

        foreach (var answer in answerResults)
            question.AddAnswerOption(answer.Value);

        var questionIdResult = await _questionsRepository.AddAsync(question, cancellationToken);
        if (questionIdResult.IsFailure)
        {
            _logger.LogWarning("Failed to create Question: {Error}", questionIdResult.Error);

            return Result.Failure<Guid>(questionIdResult.Error);
        }

        _logger.LogResult("Create Question", questionIdResult.Value, questionIdResult);

        return Result.Success(questionIdResult.Value);
    }
}