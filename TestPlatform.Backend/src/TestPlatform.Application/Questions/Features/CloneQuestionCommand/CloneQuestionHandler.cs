using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Users;

namespace TestPlatform.Application.Questions.Features.CloneQuestionCommand;

public record CloneQuestionCommand(Guid Id) : ICommand;

public sealed class CloneQuestionHandler(
    IQuestionsRepository questionsRepository,
    ICurrentUserAccessor currentUserAccessor,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CloneQuestionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CloneQuestionCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.User;
        if (currentUser is null)
        {
            return Result.Failure<Guid>(ErrorCodes.Unauthorized);
        }

        var source = await questionsRepository.GetByIdAsync(command.Id, cancellationToken);
        if (source is null)
        {
            return Result.Failure<Guid>(ErrorCodes.QuestionNotFound);
        }

        var cloneResult = source.CloneAsDraft(currentUser.Id);
        if (cloneResult.IsFailure)
        {
            return Result.Failure<Guid>(cloneResult.Error);
        }

        await questionsRepository.AddAsync(cloneResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(cloneResult.Value.Id);
    }
}
