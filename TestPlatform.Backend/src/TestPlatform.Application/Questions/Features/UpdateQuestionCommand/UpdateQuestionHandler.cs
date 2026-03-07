using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions.Features.UpdateQuestionCommand;

public record UpdateQuestionCommand(Guid Id, UpdateQuestionRequest Request) : ICommand;

public class UpdateQuestionHandler : ICommandHandler<UpdateQuestionCommand>
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly ILogger<UpdateQuestionHandler> _logger;

    public UpdateQuestionHandler(
        IQuestionsRepository questionsRepository,
        ILogger<UpdateQuestionHandler> logger)
    {
        _questionsRepository = questionsRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateQuestionCommand command, CancellationToken cancellationToken)
    {
        var questionUpdatedResult = Question.CreateWithId(
            command.Id,
            command.Request.Text,
            (QuestionType)command.Request.QuestionTypeId,
            command.Request.Points,
            command.Request.ImageUrl);
        if (questionUpdatedResult.IsFailure)
            return Result.Failure(questionUpdatedResult.Error);

        var questionUpdated = questionUpdatedResult.Value;

        foreach (var answerOption in command.Request.AnswerOptions)
        {
            var answerOptionUpdatedResult = answerOption.Id.HasValue
                ? AnswerOption.CreateWithId(answerOption.Id.Value, answerOption.Text, answerOption.IsCorrect, answerOption.ImageUrl)
                : AnswerOption.Create(answerOption.Text, answerOption.IsCorrect, answerOption.ImageUrl);
            if (answerOptionUpdatedResult.IsFailure)
                return Result.Failure(answerOptionUpdatedResult.Error);

            questionUpdated.AddAnswerOption(answerOptionUpdatedResult.Value);
        }

        foreach (var tag in command.Request.TagIds.ToHashSet())
            questionUpdated.AddTag(tag);

        var updateResult = await _questionsRepository.UpdateAsync(questionUpdated, cancellationToken);

        _logger.LogResult("Update Question", command.Id, updateResult);

        return updateResult;
    }
}
