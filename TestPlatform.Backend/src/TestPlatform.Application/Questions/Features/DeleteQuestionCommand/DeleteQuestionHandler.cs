using CSharpFunctionalExtensions;
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
        var question = await _questionsReadRepository.ReadQuestionByIdAsync(command.Id, false, cancellationToken);
        if(question == null)
            return Result.Failure("Question not found");

        if (question.ImageName is not null)
        {
            await _imageStorageService.DeletePermanentAsync(ImageFolder.QUESTIONS, question.ImageName);
            _logger.LogInformation("Deleting image {QuestionImageName}", question.ImageName);
        }

        foreach (var answer in question.AnswerOptions)
        {
            if (!string.IsNullOrEmpty(answer.ImageName))
            {
                await _imageStorageService.DeletePermanentAsync(ImageFolder.ANSWERS, answer.ImageName);
                _logger.LogInformation("Deleting answer image {AnswerImageName}", answer.ImageName);
            }
        }

        var result = await _questionsRepository.DeleteAsync(command.Id, cancellationToken);

        _logger.LogResult("Delete Question", command.Id, result);

        return result;
    }
}