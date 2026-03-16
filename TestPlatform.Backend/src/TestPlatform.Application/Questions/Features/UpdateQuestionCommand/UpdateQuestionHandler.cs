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
        var existingQuestion = await _questionsReadRepository.ReadQuestionByIdAsync(command.Id, false, cancellationToken);
        if (existingQuestion is null)
            return Result.Failure($"Question with id {command.Id} not found");

        var questionUpdatedResult = Question.CreateWithId(
            command.Id,
            command.Request.Text,
            (QuestionType)command.Request.QuestionTypeId,
            command.Request.Points,
            command.Request.ImageName);
        if (questionUpdatedResult.IsFailure)
            return Result.Failure(questionUpdatedResult.Error);

        var questionUpdated = questionUpdatedResult.Value;

        List<string> imagesForDelete = new List<string>();
        List<string> imagesForMove = new List<string>();

        var existingAnswersDict = existingQuestion.AnswerOptions
            .Where(a => a.Id != Guid.Empty)
            .ToDictionary(a => a.Id, a => a);

        foreach (var answer in command.Request.AnswerOptions)
        {
            AnswerOption answerOption;
            if (answer.Id.HasValue && existingAnswersDict.TryGetValue(answer.Id.Value, out var existingAnswer))
            {
                if (!string.IsNullOrEmpty(existingAnswer.ImageName) && existingAnswer.ImageName != answer.ImageName)
                {
                    imagesForDelete.Add(existingAnswer.ImageName);
                    if (!string.IsNullOrEmpty(answer.ImageName))
                        imagesForMove.Add(answer.ImageName);
                }
                else if (!string.IsNullOrEmpty(answer.ImageName) && string.IsNullOrEmpty(existingAnswer.ImageName))
                {
                    imagesForMove.Add(answer.ImageName);
                }

                var res = AnswerOption.CreateWithId(answer.Id.Value, answer.Text, answer.IsCorrect, answer.ImageName);
                if (res.IsFailure) return Result.Failure(res.Error);
                answerOption = res.Value;
            }
            else
            {
                var res = AnswerOption.Create(answer.Text, answer.IsCorrect, answer.ImageName);
                if (res.IsFailure) return Result.Failure(res.Error);
                answerOption = res.Value;

                if (!string.IsNullOrEmpty(answer.ImageName))
                    imagesForMove.Add(answer.ImageName);
            }

            questionUpdated.AddAnswerOption(answerOption);
        }

        var deletedAnswerImages = existingQuestion.AnswerOptions
            .Where(a => a.Id != Guid.Empty && !command.Request.AnswerOptions.Any(x => x.Id == a.Id))
            .Select(a => a.ImageName)
            .Where(img => !string.IsNullOrEmpty(img))
            .ToList();

        imagesForDelete.AddRange(deletedAnswerImages.Where(x => x != null)!);

        foreach (var tag in command.Request.TagIds.ToHashSet())
            questionUpdated.AddTag(tag);

        var updateResult = await _questionsRepository.UpdateAsync(questionUpdated, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        if (questionUpdated.ImageName != existingQuestion.ImageName)
        {
            if (!string.IsNullOrEmpty(questionUpdated.ImageName))
            {
                await _imageStorageService.MoveToPermanentAsync(questionUpdated.ImageName, ImageFolder.QUESTIONS, cancellationToken);

                _logger.LogInformation("Move question image {image}", questionUpdated.ImageName);
            }

            if (!string.IsNullOrEmpty(existingQuestion.ImageName))
            {
                await _imageStorageService.DeletePermanentAsync(ImageFolder.QUESTIONS, existingQuestion.ImageName);

                _logger.LogInformation("Delete question image {image}", existingQuestion.ImageName);
            }
        }

        foreach (string img in imagesForMove)
        {
            await _imageStorageService.MoveToPermanentAsync(img, ImageFolder.QUESTIONS, cancellationToken);

            _logger.LogInformation("Moved image {img}", img);
        }

        foreach (string img in imagesForDelete)
        {
            await _imageStorageService.DeletePermanentAsync(ImageFolder.QUESTIONS, img);

            _logger.LogInformation("Deleted image {img}", img);
        }

        _logger.LogResult("Update Question", command.Id, updateResult);

        return updateResult;
    }
}
