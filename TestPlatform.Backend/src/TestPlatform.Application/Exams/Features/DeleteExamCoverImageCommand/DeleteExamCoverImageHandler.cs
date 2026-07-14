using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.DeleteExamCoverImageCommand;

public record DeleteExamCoverImageCommand(Guid Id) : ICommand;

public class DeleteExamCoverImageHandler : ICommandHandler<DeleteExamCoverImageCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Exam> _examAccessService;
    private readonly ILogger<DeleteExamCoverImageHandler> _logger;

    public DeleteExamCoverImageHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Exam> examAccessService,
        ILogger<DeleteExamCoverImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteExamCoverImageCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var exam = accessResult.Value;

        var result = exam.RemoveCoverImage();
        if (result.IsFailure)
        {
            _logger.LogInformation("Failed to remove cover image to exam {ExamId}: {Error}", command.Id, result.Error);
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} cover image removed.", command.Id);

        return Result.Success();
    }
}