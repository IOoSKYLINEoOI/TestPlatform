using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Share;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.UpdateExamCoverImageCommand;

public record UpdateExamCoverImageCommand(Guid Id, UpdateCoverImageRequest Request) : ICommand;

public class UpdateExamCoverImageHandler : ICommandHandler<UpdateExamCoverImageCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Exam> _examAccessService;
    private readonly ILogger<UpdateExamCoverImageHandler> _logger;

    public UpdateExamCoverImageHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Exam> examAccessService,
        ILogger<UpdateExamCoverImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateExamCoverImageCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var exam = accessResult.Value;

        var result = exam.ChangeCoverImage(command.Request.FileName);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} cover image updated to {FileName}", command.Id, command.Request.FileName);

        return Result.Success();
    }
}