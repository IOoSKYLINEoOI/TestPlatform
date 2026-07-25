using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.DeleteTestTimeLimitCommand;

public record DeleteTestTimeLimitCommand(Guid Id) : ICommand;

public class DeleteTestTimeLimitHandler : ICommandHandler<DeleteTestTimeLimitCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Test> _testAccessService;
    private readonly ILogger<DeleteTestTimeLimitHandler> _logger;

    public DeleteTestTimeLimitHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Test> testAccessService,
        ILogger<DeleteTestTimeLimitHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _testAccessService = testAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTestTimeLimitCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _testAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
        {
            return accessResult;
        }

        var test = accessResult.Value;

        var result = test.RemoveTimeLimit();
        if (result.IsFailure)
        {
            _logger.LogInformation("Failed to remove time limit to test {TestId}: {Error}", command.Id, result.Error);
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Test {TestId} time limit removed.", command.Id);

        return Result.Success();
    }
}
