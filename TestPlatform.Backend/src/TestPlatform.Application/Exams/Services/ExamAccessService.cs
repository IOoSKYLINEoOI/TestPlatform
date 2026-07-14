using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Users;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Services;

public class ExamAccessService : IAccessService<Exam>
{
    private readonly IExamsRepository _examsRepository;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ILogger<ExamAccessService> _logger;

    public ExamAccessService(
        IExamsRepository examsRepository,
        ICurrentUserAccessor currentUser,
        ILogger<ExamAccessService> logger)
    {
        _examsRepository = examsRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Exam>> GetForModifyAsync(Guid id, CancellationToken ct)
    {
        var user = _currentUser.User;

        if (user is null)
        {
            _logger.LogWarning("Unauthorized access attempt to update exam {ExamId}", id);
            return Result.Failure<Exam>(ErrorCodes.Unauthorized);
        }

        var exam = await _examsRepository.GetByIdAsync(id, ct);

        if (exam is null)
        {
            _logger.LogInformation("Exam with {Id} not found.", id);
            return Result.Failure<Exam>(ErrorCodes.ExamNotFound);
        }

        if (exam.AuthorId != user.Id && !user.IsAdmin)
        {
            _logger.LogWarning("User {UserId} has no rights to update exam {ExamId}", user.Id, id);
            return Result.Failure<Exam>(ErrorCodes.Forbidden);
        }

        return Result.Success(exam);
    }
}
