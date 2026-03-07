using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Application.Tags;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions.Features.CreateQuestionCommand;

public record CreateQuestionCommand(QuestionRequest Request) : ICommand;

public class CreateQuestionHandler: ICommandHandler<Guid, CreateQuestionCommand>
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly ITagsReadRepository _tagsReadRepository;
    private readonly ILogger<CreateQuestionHandler> _logger;

    public CreateQuestionHandler(IQuestionsRepository questionsRepository, ITagsReadRepository tagsReadRepository, ILogger<CreateQuestionHandler> logger)
    {
        _questionsRepository = questionsRepository;
        _tagsReadRepository = tagsReadRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateQuestionCommand command, CancellationToken cancellationToken)
    {
        var questionResult = Question.Create(
            command.Request.Text,
            (QuestionType)command.Request.QuestionTypeId,
            command.Request.Points,
            command.Request.ImageUrl);

        if (questionResult.IsFailure)
            return Result.Failure<Guid>(questionResult.Error);

        var question = questionResult.Value;

        // SingleChoice Question
        if (question.QuestionType == QuestionType.SingleChoice)
        {
            if (command.Request.CreateAnswerOptions.Count(x => x.IsCorrect) != 1)
            {
                _logger.LogError("Failed to create Question. Invalid count of correct answers.");

                return Result.Failure<Guid>("SingleChoice question must have exactly one correct answer.");
            }
        }

        // MultiChoice Question
        if (question.QuestionType == QuestionType.MultipleChoice)
        {
            if (command.Request.CreateAnswerOptions.Count(x => x.IsCorrect) < 1)
            {
                _logger.LogError("Failed to create Question. No correct answer provided.");
                return Result.Failure<Guid>("MultipleChoice question must have at least one correct answer.");
            }
        }

        var answerResults = command.Request.CreateAnswerOptions
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

        var tagIds = command.Request.TagIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (tagIds.Any())
        {
            var existingTagIds = await _tagsReadRepository.GetExistingIdsAsync(tagIds, cancellationToken);

            var missingTagIds = tagIds
                .Except(existingTagIds)
                .ToList();

            if (missingTagIds.Any())
            {
                _logger.LogWarning("Some tags do not exist: {TagIds}", missingTagIds);
                return Result.Failure<Guid>("One or more tags do not exist.");
            }

            var tagResult = question.AddTags(existingTagIds);
            if (tagResult.IsFailure)
                return Result.Failure<Guid>(tagResult.Error);
        }

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