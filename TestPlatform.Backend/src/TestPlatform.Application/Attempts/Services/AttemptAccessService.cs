using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Users;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts.Services;

public class AttemptAccessService : IAccessService<Attempt>
{
    private readonly IAttemptsRepository _attemptsRepository;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ILogger<AttemptAccessService> _logger;

    public AttemptAccessService(
        IAttemptsRepository attemptsRepository,
        ICurrentUserAccessor currentUser,
        ILogger<AttemptAccessService> logger)
    {
        _attemptsRepository = attemptsRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Attempt>> GetForModifyAsync(Guid id, CancellationToken ct)
    {
        var user = _currentUser.User;

        if (user is null)
        {
            _logger.LogWarning("Unauthorized access attempt to update attempt {AttemptId}", id);
            return Result.Failure<Attempt>(ErrorCodes.Unauthorized);
        }

        var attempt = await _attemptsRepository.GetByIdAsync(id, ct);

        if (attempt is null)
        {
            _logger.LogInformation("Attempt with {Id} not found.", id);
            return Result.Failure<Attempt>(ErrorCodes.AttemptNotFound);
        }

        if (attempt.UserId != user.Id && !user.IsAdmin)
        {
            _logger.LogWarning("User {UserId} has no rights to update attempt {AttemptId}", user.Id, id);
            return Result.Failure<Attempt>(ErrorCodes.Forbidden);
        }

        return Result.Success(attempt);
    }
}
