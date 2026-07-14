using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Extensions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Request;
using TestPlatform.Core.Attempts;


namespace TestPlatform.Application.Attempts.Features.SaveAttemptAnswerCommand;

public record SaveAttemptAnswerCommand(Guid AttemptId, AttemptAnswerRequest Request) : ICommand;

public class SaveAttemptAnswerHandler : ICommandHandler<SaveAttemptAnswerCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Attempt> _attemptAccessService;

    public SaveAttemptAnswerHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Attempt> attemptAccessService)
    {
        _unitOfWork = unitOfWork;
        _attemptAccessService = attemptAccessService;
    }

    public async Task<Result> Handle(SaveAttemptAnswerCommand command, CancellationToken cancellationToken)
    {
        var accessResult = await _attemptAccessService.GetForModifyAsync(command.AttemptId, cancellationToken);
        if (accessResult.IsFailure)
            return Result.Failure(accessResult.Error);

        var attempt = accessResult.Value;

        var answerResult = AttemptAnswerMappingExtensions.ToDomain(command.Request);
        if (answerResult.IsFailure)
            return Result.Failure(answerResult.Error);

        var saveResult = attempt.SaveAnswer(answerResult.Value);
        if (saveResult.IsFailure)
            return saveResult;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}