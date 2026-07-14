using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Share;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.UpdateTestTimeLimitCommand;

public record UpdateTestTimeLimitCommand(Guid Id, UpdateTimeLimitRequest Request) : ICommand;

public class UpdateTestTimeLimitHandler : ICommandHandler<UpdateTestTimeLimitCommand> 
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Test> _testAccessService;
    private readonly ILogger<UpdateTestTimeLimitHandler> _logger;

    public UpdateTestTimeLimitHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Test> testAccessService,
        ILogger<UpdateTestTimeLimitHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _testAccessService = testAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTestTimeLimitCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _testAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var test = accessResult.Value;

        var result = test.ChangeTimeLimit(command.Request.TimeLimitSeconds);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Test {TestId} time limit updated to {TimeLimit}", command.Id, command.Request.TimeLimitSeconds);

        return Result.Success();
    }
}