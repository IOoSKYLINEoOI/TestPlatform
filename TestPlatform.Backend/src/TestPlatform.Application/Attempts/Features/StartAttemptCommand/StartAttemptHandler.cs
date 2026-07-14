using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Attempts.Services.SourceService;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Extensions;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts.Features.StartAttemptCommand;

public record StartAttemptCommand(StartRequest Request) : ICommand;

public class StartAttemptHandler : ICommandHandler<StartAttemptCommand, StartAttemptResponse>
{
    private readonly IAttemptsRepository _attemptsRepository;
    private readonly AttemptSourceResolver _resolver;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ILogger<StartAttemptHandler> _logger;

    public StartAttemptHandler(
        IAttemptsRepository attemptsRepository,
        AttemptSourceResolver resolver,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUser,
        ILogger<StartAttemptHandler> logger)
    {
        _attemptsRepository = attemptsRepository;
        _resolver = resolver;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<StartAttemptResponse>> Handle(StartAttemptCommand command, CancellationToken cancellationToken)
    {
        var user = _currentUser.User;

        if (user is null)
        {
            _logger.LogWarning("Unauthorized access attempt to create exam.");
            return Result.Failure<StartAttemptResponse>(ErrorCodes.Unauthorized);
        }

        var request = command.Request;
        var attemptType = request.Type.ToDomain();

        var sourceResult = await _resolver.GetSourceAsync(
            attemptType,
            request.SourceId,
            cancellationToken);

        if (sourceResult.IsFailure)
            return Result.Failure<StartAttemptResponse>(sourceResult.Error);

        var source = sourceResult.Value;

        var attemptResult = Attempt.Create(
            user.Id,
            request.Type.ToDomain(),
            request.SourceId,
            source.TotalQuestions,
            source.TotalMaxScore,
            source.TimeLimitSeconds);

        if (attemptResult.IsFailure)
            return Result.Failure<StartAttemptResponse>(attemptResult.Error);

        var attempt = attemptResult.Value;

        var startResult = attempt.Start();
        if (startResult.IsFailure)
            return Result.Failure<StartAttemptResponse>(startResult.Error);

        await _attemptsRepository.AddAsync(attempt, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new StartAttemptResponse(
            attempt.Id,
            AttemptStartMapper.ToStartResponse(source, attemptType));

        _logger.LogResult("Attempt started", attempt.Id, attemptResult);

        return Result.Success(response);
    }
}