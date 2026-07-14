/*using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Abstractions.Enums;
using TestPlatform.Application.Extensions;

namespace TestPlatform.Application.Questions.Features.DeleteQuestionCommand;

public record DeleteQuestionCommand(Guid Id) : ICommand;

public class DeleteQuestionHandler : ICommandHandler<DeleteQuestionCommand>
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly IQuestionsReadRepository _questionsReadRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<DeleteQuestionHandler> _logger;

    public DeleteQuestionHandler(
        IQuestionsRepository questionsRepository,
        IQuestionsReadRepository questionsReadRepository,
        IImageStorageService imageStorageService,
        ILogger<DeleteQuestionHandler> logger)
    {
        _questionsRepository = questionsRepository;
        _questionsReadRepository = questionsReadRepository;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteQuestionCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty)
            return Result.Failure("Invalid question Id");

        var question = await _questionsReadRepository.ReadQuestionByIdAsync(command.Id, false, cancellationToken);
        if (question == null)
            return Result.Failure("Question not found");

        if (!string.IsNullOrWhiteSpace(question.ImageName))
        {
            var deleteQuestionImageResult = await _imageStorageService.DeletePermanentAsync(
                ImageFolder.QUESTIONS, question.ImageName);

            if (deleteQuestionImageResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to delete question image {QuestionImageName}: {Error}",
                    question.ImageName,
                    deleteQuestionImageResult.Error);
            }
            else
            {
                _logger.LogInformation("Deleted question image {QuestionImageName}", question.ImageName);
            }
        }

        var answerDeleteTasks = question.AnswerOptions
            .Where(a => !string.IsNullOrWhiteSpace(a.ImageName))
            .Select(async answer =>
            {
                if (answer.ImageName != null)
                {
                    var result = await _imageStorageService.DeletePermanentAsync(
                        ImageFolder.ANSWERS,
                        answer.ImageName);
                    if (result.IsFailure)
                    {
                        _logger.LogWarning("Failed to delete answer image {AnswerImageName}: {Error}", answer.ImageName, result.Error);
                    }
                    else
                    {
                        _logger.LogInformation("Deleted answer image {AnswerImageName}", answer.ImageName);
                    }
                }
            });

        await Task.WhenAll(answerDeleteTasks);

        var deleteResult = await _questionsRepository.DeleteAsync(command.Id, cancellationToken);

        _logger.LogResult("Delete Question", command.Id, deleteResult);

        return deleteResult;
    }
}*/