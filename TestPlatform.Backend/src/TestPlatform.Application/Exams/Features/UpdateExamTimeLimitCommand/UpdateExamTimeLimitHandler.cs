using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Common;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.UpdateExamTimeLimitCommand;

public record UpdateExamTimeLimitCommand(Guid Id, UpdateTimeLimitRequest Request) : ICommand;

public class UpdateExamTimeLimitHandler : ICommandHandler<UpdateExamTimeLimitCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Exam> _examAccessService;
    private readonly ILogger<UpdateExamTimeLimitHandler> _logger;

    public UpdateExamTimeLimitHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Exam> examAccessService,
        ILogger<UpdateExamTimeLimitHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateExamTimeLimitCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
        {
            return accessResult;
        }

        var exam = accessResult.Value;

        var result = exam.ChangeTimeLimit(command.Request.TimeLimitSeconds);
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} time limit updated to {TimeLimit}", command.Id, command.Request.TimeLimitSeconds);

        return Result.Success();
    }
}
