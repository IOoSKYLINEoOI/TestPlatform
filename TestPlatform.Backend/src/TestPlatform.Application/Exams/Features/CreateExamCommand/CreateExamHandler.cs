using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Extensions;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Exams.DTOs;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.CreateExamCommand;

public record CreateExamCommand(ExamRequest Request) : ICommand;

public class CreateExamHandler : ICommandHandler<CreateExamCommand, Guid>
{
    private readonly IExamsRepository _examsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ILogger<CreateExamHandler> _logger;

    public CreateExamHandler(
        IExamsRepository examsRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUser,
        ILogger<CreateExamHandler> logger)
    {
        _examsRepository = examsRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateExamCommand command, CancellationToken cancellationToken = default)
    {
        var user = _currentUser.User;

        if (user is null)
        {
            _logger.LogWarning("Unauthorized access attempt to create exam.");
            return Result.Failure<Guid>(ErrorCodes.Unauthorized);
        }

        var examResult = Exam.Create(
            command.Request.Title,
            command.Request.Description,
            user.Id);

        if (examResult.IsFailure)
        {
            return Result.Failure<Guid>(examResult.Error);
        }

        await _examsRepository.AddAsync(examResult.Value, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogResult("Create Exam", examResult.Value.Id, examResult);

        return Result.Success(examResult.Value.Id);
    }
}
