using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.DeleteExamTimeLimitCommand;

public record DeleteExamTimeLimitCommand(Guid Id) : ICommand;

public class DeleteExamTimeLimitHandler : ICommandHandler<DeleteExamTimeLimitCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Exam> _examAccessService;
    private readonly ILogger<DeleteExamTimeLimitHandler> _logger;

    public DeleteExamTimeLimitHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Exam> examAccessService,
        ILogger<DeleteExamTimeLimitHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteExamTimeLimitCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var exam = accessResult.Value;

        var result = exam.RemoveTimeLimit();
        if (result.IsFailure)
        {
            _logger.LogInformation("Failed to remove time limit to exam {ExamId}: {Error}", command.Id, result.Error);
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} time limit removed.", command.Id);

        return Result.Success();
    }
}