using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Users;

namespace TestPlatform.Application.Attempts.Features.CancelAttemptCommand;

public record CancelAttemptCommand(Guid AttemptId) : ICommand;

public sealed class CancelAttemptHandler(
    IAttemptsRepository attemptsRepository,
    ICurrentUserAccessor currentUserAccessor,
    IUnitOfWork unitOfWork) : ICommandHandler<CancelAttemptCommand>
{
    public async Task<Result> Handle(CancelAttemptCommand command, CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.User;
        if (user is null)
        {
            return Result.Failure(ErrorCodes.Unauthorized);
        }

        if (!user.IsAdmin)
        {
            return Result.Failure(ErrorCodes.Forbidden);
        }

        var attempt = await attemptsRepository.GetByIdAsync(command.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure(ErrorCodes.AttemptNotFound);
        }

        var result = attempt.Cancel();
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
