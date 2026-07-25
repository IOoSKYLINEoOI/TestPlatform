using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.ArchiveTestCommand;

public record ArchiveTestCommand(Guid Id) : ICommand;

public sealed class ArchiveTestHandler(
    IAccessService<Test> testAccessService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ArchiveTestCommand>
{
    public async Task<Result> Handle(ArchiveTestCommand command, CancellationToken cancellationToken)
    {
        var accessResult = await testAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure(accessResult.Error);
        }

        var archiveResult = accessResult.Value.Archive();
        if (archiveResult.IsFailure)
        {
            return archiveResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
