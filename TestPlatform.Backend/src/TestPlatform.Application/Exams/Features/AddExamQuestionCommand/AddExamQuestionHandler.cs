using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Exams.Services;
using TestPlatform.Application.Questions;
using TestPlatform.Contracts.Exams.DTOs;

namespace TestPlatform.Application.Exams.Features.AddExamQuestionCommand;

public record AddExamQuestionCommand(Guid Id, AddExamQuestionRequest Request) : ICommand;

public class AddExamQuestionHandler : ICommandHandler<AddExamQuestionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExamAccessService _examAccessService;
    private readonly IQuestionsRepository _questionsRepository;
    private readonly ILogger<AddExamQuestionHandler> _logger;

    public AddExamQuestionHandler(
        IUnitOfWork unitOfWork,
        IExamAccessService examAccessService,
        IQuestionsRepository questionsRepository,
        ILogger<AddExamQuestionHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _questionsRepository = questionsRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(AddExamQuestionCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var exam = accessResult.Value;

        bool questionExists = await _questionsRepository.ExistsAsync(command.Request.QuestionId, cancellationToken);
        if (questionExists is false)
        {
            _logger.LogInformation("Question with {Id} not found.", command.Request.QuestionId);
            return Result.Failure(ErrorCodes.QuestionNotFound);
        }

        var result = exam.AddQuestion(command.Request.QuestionId, command.Request.Score);
        if (result.IsFailure)
        {
            _logger.LogInformation("Failed to add question to exam {ExamId}: {Error}", command.Id, result.Error);
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} question add.", command.Id);

        return Result.Success();
    }
}