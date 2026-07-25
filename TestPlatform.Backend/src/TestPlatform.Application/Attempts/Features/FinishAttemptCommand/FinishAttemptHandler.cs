using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Attempts.Services;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Features.FinishAttemptCommand;

public record FinishAttemptCommand(Guid AttemptId) : ICommand;

public class FinishAttemptHandler : ICommandHandler<FinishAttemptCommand, AttemptResultResponse>
{
    private readonly IAccessService<Attempt> _attemptAccessService;
    private readonly AttemptQuestionLoader _questionLoader;
    private readonly IUnitOfWork _unitOfWork;

    public FinishAttemptHandler(
        IAccessService<Attempt> attemptAccessService,
        AttemptQuestionLoader questionLoader,
        IUnitOfWork unitOfWork)
    {
        _attemptAccessService = attemptAccessService;
        _questionLoader = questionLoader;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AttemptResultResponse>> Handle(
        FinishAttemptCommand command,
        CancellationToken cancellationToken)
    {
        var attemptResult = await _attemptAccessService
            .GetForModifyAsync(command.AttemptId, cancellationToken);

        if (attemptResult.IsFailure)
        {
            return Result.Failure<AttemptResultResponse>(attemptResult.Error);
        }

        var attempt = attemptResult.Value;

        var questionsResult = await _questionLoader.LoadAsync(
            attempt.QuestionSelections,
            cancellationToken);

        if (questionsResult.IsFailure)
        {
            return Result.Failure<AttemptResultResponse>(questionsResult.Error);
        }

        var finishResult = attempt.Finish(
            questionsResult.Value);

        if (finishResult.IsFailure)
        {
            if (attempt.Status == AttemptStatus.EXPIRED)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Failure<AttemptResultResponse>(finishResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(attempt.ToResultResponse());
    }
}
