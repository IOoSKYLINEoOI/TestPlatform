using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.ReorderTestQuestionsCommand;

public sealed record ReorderTestQuestionsCommand(Guid TestId, IReadOnlyList<Guid> QuestionIds) : ICommand;

public sealed class ReorderTestQuestionsHandler(
    IAccessService<Test> testAccessService,
    IUnitOfWork unitOfWork) : ICommandHandler<ReorderTestQuestionsCommand>
{
    public async Task<Result> Handle(ReorderTestQuestionsCommand command, CancellationToken cancellationToken)
    {
        var access = await testAccessService.GetForModifyAsync(command.TestId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var prepareResult = access.Value.PrepareQuestionReorder(command.QuestionIds);
        if (prepareResult.IsFailure)
        {
            return prepareResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var result = access.Value.ReorderQuestions(command.QuestionIds);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
