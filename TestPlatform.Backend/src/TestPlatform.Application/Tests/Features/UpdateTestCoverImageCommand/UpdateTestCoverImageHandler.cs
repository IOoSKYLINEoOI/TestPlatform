using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Files;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Common;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.UpdateTestCoverImageCommand;

public record UpdateTestCoverImageCommand(Guid Id, UpdateCoverImageRequest Request) : ICommand;

public class UpdateTestCoverImageHandler : ICommandHandler<UpdateTestCoverImageCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Test> _testAccessService;
    private readonly IFileAssetService _fileAssetService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILogger<UpdateTestCoverImageHandler> _logger;

    public UpdateTestCoverImageHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Test> testAccessService,
        IFileAssetService fileAssetService,
        ICurrentUserAccessor currentUserAccessor,
        ILogger<UpdateTestCoverImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _testAccessService = testAccessService;
        _fileAssetService = fileAssetService;
        _currentUserAccessor = currentUserAccessor;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTestCoverImageCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _testAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
        {
            return accessResult;
        }

        var currentUser = _currentUserAccessor.User;
        if (currentUser is null)
        {
            return Result.Failure("unauthorized");
        }

        var attachResult = await _fileAssetService.AttachAsync(
            command.Request.FileId,
            currentUser.Id,
            cancellationToken);

        if (attachResult.IsFailure)
        {
            return Result.Failure(attachResult.Error);
        }

        var test = accessResult.Value;
        var previousFileId = test.CoverImageId;

        var result = test.ChangeCoverImage(command.Request.FileId);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (previousFileId.HasValue && previousFileId != command.Request.FileId)
        {
            await _fileAssetService.ReleaseIfUnreferencedAsync(previousFileId.Value, cancellationToken);
        }

        _logger.LogInformation("Test {TestId} cover image updated to {FileId}", command.Id, command.Request.FileId);

        return Result.Success();
    }
}
