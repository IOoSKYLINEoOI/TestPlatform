using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.PublishTestCommand;

public record PublishTestCommand(Guid Id) : ICommand;

public sealed class PublishTestHandler(
    IAccessService<Test> testAccessService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<PublishTestCommand>
{
    public async Task<Result> Handle(PublishTestCommand command, CancellationToken cancellationToken)
    {
        var accessResult = await testAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure(accessResult.Error);
        }

        var publishResult = accessResult.Value.Publish();
        if (publishResult.IsFailure)
        {
            return publishResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
