using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Features.RemoveAttemptAnswerCommand;

public record RemoveAttemptAnswerCommand(Guid AttemptId, Guid QuestionId) : ICommand;

public sealed class RemoveAttemptAnswerHandler(
    IAccessService<Attempt> accessService,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveAttemptAnswerCommand>
{
    public async Task<Result> Handle(RemoveAttemptAnswerCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.GetForModifyAsync(command.AttemptId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var result = access.Value.RemoveAnswer(command.QuestionId);
        if (result.IsFailure)
        {
            if (access.Value.Status == AttemptStatus.EXPIRED)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
