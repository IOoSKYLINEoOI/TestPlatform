using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Abstractions.Enums;
using TestPlatform.Application.Extensions;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions.Features.UpdateQuestionCommand;

public record UpdateQuestionCommand(Guid Id, UpdateQuestionRequest Request) : ICommand;

public class UpdateQuestionHandler : ICommandHandler<UpdateQuestionCommand>
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly IQuestionsReadRepository _questionsReadRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<UpdateQuestionHandler> _logger;

    public UpdateQuestionHandler(
        IQuestionsRepository questionsRepository,
        IQuestionsReadRepository questionsReadRepository,
        IImageStorageService imageStorageService,
        ILogger<UpdateQuestionHandler> logger)
    {
        _questionsRepository = questionsRepository;
        _questionsReadRepository = questionsReadRepository;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateQuestionCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty)
            return Result.Failure("Invalid question Id");

        var existingQuestion = await _questionsReadRepository.ReadQuestionByIdAsync(
            command.Id, false, cancellationToken);

        if (existingQuestion is null)
            return Result.Failure($"Question with id {command.Id} not found");

        var questionResult = Question.CreateWithId(
            command.Id,
            command.Request.Text,
            (QuestionType)command.Request.QuestionTypeId,
            command.Request.Points,
            command.Request.ImageName);

        if (questionResult.IsFailure)
            return Result.Failure(questionResult.Error);

        var updatedQuestion = questionResult.Value;

        var existingAnswersDict = existingQuestion.AnswerOptions
            .Where(a => a.Id != Guid.Empty)
            .ToDictionary(a => a.Id, a => a);

        var imagesForMove = new List<string>();
        var imagesForDelete = new List<string>();

        foreach (var answerDto in command.Request.AnswerOptions)
        {
            AnswerOption answer;

            if (answerDto.Id.HasValue && existingAnswersDict.TryGetValue(answerDto.Id.Value, out var existingAnswer))
            {
                if (!string.IsNullOrEmpty(existingAnswer.ImageName) &&
                    existingAnswer.ImageName != answerDto.ImageName)
                {
                    imagesForDelete.Add(existingAnswer.ImageName);
                    if (!string.IsNullOrEmpty(answerDto.ImageName))
                        imagesForMove.Add(answerDto.ImageName);
                }
                else if (!string.IsNullOrEmpty(answerDto.ImageName) &&
                         string.IsNullOrEmpty(existingAnswer.ImageName))
                {
                    imagesForMove.Add(answerDto.ImageName);
                }

                var res = AnswerOption.CreateWithId(
                    answerDto.Id.Value,
                    answerDto.Text,
                    answerDto.IsCorrect,
                    answerDto.ImageName);

                if (res.IsFailure) return Result.Failure(res.Error);
                answer = res.Value;
            }
            else
            {
                var res = AnswerOption.Create(
                    answerDto.Text,
                    answerDto.IsCorrect,
                    answerDto.ImageName);

                if (res.IsFailure) return Result.Failure(res.Error);
                answer = res.Value;

                if (!string.IsNullOrEmpty(answerDto.ImageName))
                    imagesForMove.Add(answerDto.ImageName);
            }

            updatedQuestion.AddAnswerOption(answer);
        }

        var deletedAnswerImages = existingQuestion.AnswerOptions
            .Where(a => a.Id != Guid.Empty && !command.Request.AnswerOptions.Any(x => x.Id == a.Id))
            .Select(a => a.ImageName)
            .Where(img => !string.IsNullOrEmpty(img));

        imagesForDelete.AddRange(deletedAnswerImages!);

        foreach (var tagId in command.Request.TagIds.ToHashSet())
            updatedQuestion.AddTag(tagId);

        var updateResult = await _questionsRepository.UpdateAsync(updatedQuestion, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        if (updatedQuestion.ImageName != existingQuestion.ImageName)
        {
            if (!string.IsNullOrEmpty(updatedQuestion.ImageName))
            {
                await _imageStorageService.MoveToPermanent(updatedQuestion.ImageName, ImageFolder.QUESTIONS);
                _logger.LogInformation("Moved question image {Image}", updatedQuestion.ImageName);
            }

            if (!string.IsNullOrEmpty(existingQuestion.ImageName))
            {
                await _imageStorageService.DeletePermanentAsync(ImageFolder.QUESTIONS, existingQuestion.ImageName);
                _logger.LogInformation("Deleted old question image {Image}", existingQuestion.ImageName);
            }
        }

        foreach (var img in imagesForMove)
        {
            await _imageStorageService.MoveToPermanent(img, ImageFolder.ANSWERS);
            _logger.LogInformation("Moved answer image {Image}", img);
        }

        foreach (var img in imagesForDelete)
        {
            await _imageStorageService.DeletePermanentAsync(ImageFolder.ANSWERS, img);
            _logger.LogInformation("Deleted answer image {Image}", img);
        }

        _logger.LogResult("Update Question", command.Id, updateResult);

        return updateResult;
    }
}
