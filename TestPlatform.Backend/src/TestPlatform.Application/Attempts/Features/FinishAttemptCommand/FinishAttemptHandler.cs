using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Attempts.Services.SourceService;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts.Features.FinishAttemptCommand;

public record FinishAttemptCommand(Guid AttemptId) : ICommand;

public class FinishAttemptHandler : ICommandHandler<FinishAttemptCommand, FinishAttemptResponse>
{
    private readonly IAccessService<Attempt> _attemptAccessService;
    private readonly AttemptSourceResolver _resolver;
    private readonly IUnitOfWork _unitOfWork;

    public FinishAttemptHandler(
        IAccessService<Attempt> attemptAccessService,
        AttemptSourceResolver resolver,
        IUnitOfWork unitOfWork)
    {
        _attemptAccessService = attemptAccessService;
        _resolver = resolver;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FinishAttemptResponse>> Handle(
        FinishAttemptCommand command,
        CancellationToken cancellationToken)
    {
        var attemptResult = await _attemptAccessService
            .GetForModifyAsync(command.AttemptId, cancellationToken);

        if (attemptResult.IsFailure)
            return Result.Failure<FinishAttemptResponse>(attemptResult.Error);

        var attempt = attemptResult.Value;

        var sourceResult = await _resolver.GetSourceAsync(
            attempt.Type,
            attempt.SourceId,
            cancellationToken);

        if (sourceResult.IsFailure)
            return Result.Failure<FinishAttemptResponse>(sourceResult.Error);

        var finishResult = attempt.Finish(
            sourceResult.Value.Questions);

        if (finishResult.IsFailure)
            return Result.Failure<FinishAttemptResponse>(finishResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(AttemptFinishMapper.ToFinishResponse(attempt));
    }
}