using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Exams.DTOs;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.UpdateExamDetailsCommand;

public record UpdateExamDetailsCommand(Guid Id, UpdateExamDetailsRequest Request) : ICommand;

public class UpdateExamDetailsHandler : ICommandHandler<UpdateExamDetailsCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Exam> _examAccessService;
    private readonly ILogger<UpdateExamDetailsHandler> _logger;

    public UpdateExamDetailsHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Exam> examAccessService,
        ILogger<UpdateExamDetailsHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateExamDetailsCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
        {
            return accessResult;
        }

        var exam = accessResult.Value;

        if (command.Request.Title != null)
        {
            var changeTitleResult = exam.ChangeTitle(command.Request.Title);
            if (changeTitleResult.IsFailure)
            {
                return Result.Failure(changeTitleResult.Error);
            }
        }

        if (command.Request.Description != null)
        {
            var changeDescriptionResult = exam.ChangeDescription(command.Request.Description);
            if (changeDescriptionResult.IsFailure)
            {
                return Result.Failure(changeDescriptionResult.Error);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} title and description updated.", command.Id);

        return Result.Success();
    }
}
