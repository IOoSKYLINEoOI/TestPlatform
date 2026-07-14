using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.DeleteTestCoverImageCommand;

public record DeleteTestCoverImageCommand(Guid Id) : ICommand;

public class DeleteTestCoverImageHandler : ICommandHandler<DeleteTestCoverImageCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Test> _testAccessService;
    private readonly ILogger<DeleteTestCoverImageHandler> _logger;

    public DeleteTestCoverImageHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Test> testAccessService,
        ILogger<DeleteTestCoverImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _testAccessService = testAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTestCoverImageCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _testAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var test = accessResult.Value;

        var result = test.RemoveCoverImage();
        if (result.IsFailure)
        {
            _logger.LogInformation("Failed to remove cover image to test {TestId}: {Error}", command.Id, result.Error);
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Test {TestId} cover image removed.", command.Id);

        return Result.Success();
    }
}