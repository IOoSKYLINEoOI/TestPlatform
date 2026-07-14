using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.DeleteExamQuestionCommand;

public record DeleteExamQuestionCommand(Guid Id, Guid QuestionId) : ICommand;

public class DeleteExamQuestionHandler : ICommandHandler<DeleteExamQuestionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Exam> _examAccessService;
    private readonly ILogger<DeleteExamQuestionHandler> _logger;

    public DeleteExamQuestionHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Exam> examAccessService,
        ILogger<DeleteExamQuestionHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteExamQuestionCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var exam = accessResult.Value;

        var result = exam.RemoveQuestion(command.QuestionId);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} question {QuestionId} deleted.", command.Id, command.QuestionId);

        return Result.Success();
    }
}