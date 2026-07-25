using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts.Features.AbandonAttemptCommand;

public record AbandonAttemptCommand(Guid AttemptId) : ICommand;

public class AbandonAttemptHandler(
    IAccessService<Attempt> attemptAccessService,
    IUnitOfWork unitOfWork) : ICommandHandler<AbandonAttemptCommand>
{
    public async Task<Result> Handle(
        AbandonAttemptCommand command,
        CancellationToken cancellationToken)
    {
        var accessResult = await attemptAccessService.GetForModifyAsync(
            command.AttemptId,
            cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure(accessResult.Error);
        }

        var result = accessResult.Value.Abandon();
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
