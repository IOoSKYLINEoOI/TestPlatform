using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Application.Attempts.Features.GetByIdAttemptQuery;

public record GetByIdAttemptQuery(Guid Id, CurrentUserDto CurrentUser) : IQuery;

public class GetByIdAttemptHandler : IQueryHandler<AttemptResponse, GetByIdAttemptQuery>
{
    private readonly IAttemptsReadRepository _attemptsReadRepository;
    private readonly ILogger<GetByIdAttemptHandler> _logger;

    public GetByIdAttemptHandler(IAttemptsReadRepository attemptsReadRepository, ILogger<GetByIdAttemptHandler> logger)
    {
        _attemptsReadRepository = attemptsReadRepository;
        _logger = logger;
    }

    public async Task<Result<AttemptResponse>> Handle(GetByIdAttemptQuery query, CancellationToken cancellationToken)
    {
        var attempt = await _attemptsReadRepository.ReadAttemptByIdAsync(query.Id, cancellationToken);

        if (attempt == null)
        {
            _logger.LogWarning("Attempt with id {Id} not found", query.Id);
            return Result.Failure<AttemptResponse>("attempt.not_found");
        }

        if (query.CurrentUser.Id != attempt.UserId && !query.CurrentUser.IsAdmin)
        {
            _logger.LogInformation("Unauthorized access by user {UserId}", query.CurrentUser.Id);
            return Result.Failure<AttemptResponse>("unauthorized");
        }

        _logger.LogInformation("Get Attempt with id {Id}", query.Id);
        return Result.Success(attempt);
    }
}