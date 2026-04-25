using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Exams.Services;

namespace TestPlatform.Application.Exams.Features.DeleteExamScheduleCommand;

public record DeleteExamScheduleCommand(Guid Id) : ICommand;

public class DeleteExamScheduleHandler : ICommandHandler<DeleteExamScheduleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExamAccessService _examAccessService;
    private readonly ILogger<DeleteExamScheduleHandler> _logger;

    public DeleteExamScheduleHandler(
        IUnitOfWork unitOfWork,
        IExamAccessService examAccessService,
        ILogger<DeleteExamScheduleHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteExamScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var exam = accessResult.Value;

        var result = exam.RemoveSchedule();
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} schedule removed", command.Id);

        return Result.Success();
    }
}