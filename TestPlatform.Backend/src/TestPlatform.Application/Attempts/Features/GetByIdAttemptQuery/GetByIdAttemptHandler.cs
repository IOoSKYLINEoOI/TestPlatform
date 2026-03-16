using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Attempts.DTOs;

namespace TestPlatform.Application.Attempts.Features.GetByIdAttemptQuery;

public record GetByIdAttemptQuery(Guid Id) : IQuery;

public class GetByIdAttemptHandler : IQueryHandler<AttemptResponse, GetByIdAttemptQuery>
{
    private readonly IAttemptsReadRepository _attemptsReadRepository;
    private readonly ILogger<GetByIdAttemptHandler> _logger;

    public GetByIdAttemptHandler(IAttemptsReadRepository attemptsReadRepository, ILogger<GetByIdAttemptHandler> logger)
    {
        _attemptsReadRepository = attemptsReadRepository;
        _logger = logger;
    }

    public async Task<AttemptResponse?> Handle(GetByIdAttemptQuery query, CancellationToken cancellationToken)
    {
        var attempt =
            await _attemptsReadRepository.ReadAttemptByIdAsync(query.Id, cancellationToken);

        if (attempt == null)
            _logger.LogWarning("Attempt with id {Id} not found", query.Id);
        else
            _logger.LogInformation("Get Attempt with id {Id}", query.Id);

        return attempt;
    }
}