using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Files;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Common;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.UpdateExamCoverImageCommand;

public record UpdateExamCoverImageCommand(Guid Id, UpdateCoverImageRequest Request) : ICommand;

public class UpdateExamCoverImageHandler : ICommandHandler<UpdateExamCoverImageCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Exam> _examAccessService;
    private readonly IFileAssetService _fileAssetService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILogger<UpdateExamCoverImageHandler> _logger;

    public UpdateExamCoverImageHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Exam> examAccessService,
        IFileAssetService fileAssetService,
        ICurrentUserAccessor currentUserAccessor,
        ILogger<UpdateExamCoverImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _fileAssetService = fileAssetService;
        _currentUserAccessor = currentUserAccessor;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateExamCoverImageCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
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

        var exam = accessResult.Value;
        var previousFileId = exam.CoverImageId;

        var result = exam.ChangeCoverImage(command.Request.FileId);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (previousFileId.HasValue && previousFileId != command.Request.FileId)
        {
            await _fileAssetService.ReleaseIfUnreferencedAsync(previousFileId.Value, cancellationToken);
        }

        _logger.LogInformation("Exam {ExamId} cover image updated to {FileId}", command.Id, command.Request.FileId);

        return Result.Success();
    }
}
