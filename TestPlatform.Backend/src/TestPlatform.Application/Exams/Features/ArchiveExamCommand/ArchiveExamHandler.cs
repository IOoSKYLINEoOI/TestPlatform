using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Exams.Services;
using TestPlatform.Application.Users;

namespace TestPlatform.Application.Exams.Features.ArchiveExamCommand;

public record ArchiveExamCommand(Guid Id) : ICommand;

public class ArchiveExamHandler : ICommandHandler<ArchiveExamCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExamAccessService _examAccessService;
    private readonly ILogger<ArchiveExamHandler> _logger;

    public ArchiveExamHandler(
        IUnitOfWork unitOfWork,
        IExamAccessService examAccessService,
        ILogger<ArchiveExamHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(ArchiveExamCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var exam = accessResult.Value;

        var result = exam.Archive();
        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} archived.", command.Id);

        return Result.Success();
    }
}