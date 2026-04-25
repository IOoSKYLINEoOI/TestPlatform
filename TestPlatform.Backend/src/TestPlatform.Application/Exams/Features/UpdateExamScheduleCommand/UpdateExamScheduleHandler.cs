using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Exams.Services;
using TestPlatform.Contracts.Exams.DTOs;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.UpdateExamScheduleCommand;

public record UpdateExamScheduleCommand(Guid Id, UpdateExamScheduleRequest Request) : ICommand;

public class UpdateExamScheduleHandler : ICommandHandler<UpdateExamScheduleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExamAccessService _examAccessService;
    private readonly ILogger<UpdateExamScheduleHandler> _logger;

    public UpdateExamScheduleHandler(
        IUnitOfWork unitOfWork,
        IExamAccessService examAccessService,
        ILogger<UpdateExamScheduleHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateExamScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var exam = accessResult.Value;

        var scheduleResult = ExamSchedule.Create(command.Request.AvailableFrom, command.Request.AvailableTo);
        if (scheduleResult.IsFailure)
            return scheduleResult;

        var result = exam.ChangeSchedule(scheduleResult.Value);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} schedule updated from {From} to {To}", command.Id, command.Request.AvailableFrom, command.Request.AvailableTo);

        return Result.Success();
    }
}