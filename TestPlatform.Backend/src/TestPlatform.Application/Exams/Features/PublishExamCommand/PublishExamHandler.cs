using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Exams.Services;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.PublishExamCommand;

public record PublishExamCommand(Guid Id) : ICommand;

public class PublishExamHandler : ICommandHandler<PublishExamCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Exam> _examAccessService;
    private readonly ILogger<PublishExamHandler> _logger;

    public PublishExamHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Exam> examAccessService,
        ILogger<PublishExamHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(PublishExamCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var exam = accessResult.Value;

        var result = exam.Publish();
        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} published.", command.Id);

        return Result.Success(command.Id);
    }
}