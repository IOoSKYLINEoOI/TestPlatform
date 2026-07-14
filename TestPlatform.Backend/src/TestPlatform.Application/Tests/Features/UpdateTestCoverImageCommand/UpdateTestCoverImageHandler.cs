using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Share;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.UpdateTestCoverImageCommand;

public record UpdateTestCoverImageCommand(Guid Id, UpdateCoverImageRequest Request) : ICommand;

public class UpdateTestCoverImageHandler : ICommandHandler<UpdateTestCoverImageCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Test> _testAccessService;
    private readonly ILogger<UpdateTestCoverImageHandler> _logger;

    public UpdateTestCoverImageHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Test> testAccessService,
        ILogger<UpdateTestCoverImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _testAccessService = testAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTestCoverImageCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _testAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var test = accessResult.Value;

        var result = test.ChangeCoverImage(command.Request.FileName);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Test {TestId} cover image updated to {FileName}", command.Id, command.Request.FileName);

        return Result.Success();
    }
}