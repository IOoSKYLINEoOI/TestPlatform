using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Attempts.Services;
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
    private readonly IAttemptStartStore _attemptStartStore;
    private readonly AttemptSourceResolver _resolver;
    private readonly AttemptQuestionLoader _questionLoader;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ILogger<StartAttemptHandler> _logger;

    public StartAttemptHandler(
        IAttemptStartStore attemptStartStore,
        AttemptSourceResolver resolver,
        AttemptQuestionLoader questionLoader,
        ICurrentUserAccessor currentUser,
        ILogger<StartAttemptHandler> logger)
    {
        _attemptStartStore = attemptStartStore;
        _resolver = resolver;
        _questionLoader = questionLoader;
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

        if (request.RequestId == Guid.Empty)
        {
            return Result.Failure<StartAttemptResponse>("attempt.request_id_required");
        }

        var existing = await _attemptStartStore.FindByRequestIdAsync(
            user.Id,
            request.RequestId,
            cancellationToken);
        if (existing is not null)
        {
            if (existing.Type != attemptType || existing.SourceId != request.SourceId)
            {
                return Result.Failure<StartAttemptResponse>("attempt.request_id_conflict");
            }

            return await MapStoredAttemptAsync(existing, cancellationToken);
        }

        var sourceResult = await _resolver.GetSourceAsync(
            attemptType,
            request.SourceId,
            cancellationToken);

        if (sourceResult.IsFailure)
        {
            return Result.Failure<StartAttemptResponse>(sourceResult.Error);
        }

        var source = sourceResult.Value;

        var attemptResult = Attempt.Create(
            user.Id,
            request.Type.ToDomain(),
            request.SourceId,
            source.Questions
                .Select(x => new AttemptQuestionSelection(x.Question.Id, x.Order, x.Score))
                .ToList(),
            source.TimeLimitSeconds,
            source.MinPassingScore,
            source.MinPassingPercent,
            source.AvailableTo,
            source.ReviewAvailableAt,
            request.RequestId);

        if (attemptResult.IsFailure)
        {
            return Result.Failure<StartAttemptResponse>(attemptResult.Error);
        }

        var attempt = attemptResult.Value;

        var startResult = attempt.Start();
        if (startResult.IsFailure)
        {
            return Result.Failure<StartAttemptResponse>(startResult.Error);
        }

        var addResult = await _attemptStartStore.AddAsync(
            attempt,
            source.AttemptsLimit,
            cancellationToken);
        if (addResult.IsFailure)
        {
            return Result.Failure<StartAttemptResponse>(addResult.Error);
        }

        attempt = addResult.Value.Attempt;

        if (!addResult.Value.Created)
        {
            return await MapStoredAttemptAsync(attempt, cancellationToken);
        }

        var response = new StartAttemptResponse(
            attempt.Id,
            attempt.AttemptNumber,
            attempt.Status.ToDto(),
            AttemptStartMapper.ToStartResponse(source, attemptType));

        _logger.LogResult("Attempt started", attempt.Id, attemptResult);

        return Result.Success(response);
    }

    private async Task<Result<StartAttemptResponse>> MapStoredAttemptAsync(
        Attempt attempt,
        CancellationToken cancellationToken)
    {
        var storedQuestions = await _questionLoader.LoadAsync(
            attempt.QuestionSelections,
            cancellationToken);
        if (storedQuestions.IsFailure)
        {
            return Result.Failure<StartAttemptResponse>(storedQuestions.Error);
        }

        var source = new AttemptSource(
            storedQuestions.Value,
            attempt.TotalQuestions,
            attempt.TotalMaxScore,
            attempt.TimeLimitSeconds);
        return Result.Success(new StartAttemptResponse(
            attempt.Id,
            attempt.AttemptNumber,
            attempt.Status.ToDto(),
            AttemptStartMapper.ToStartResponse(source, attempt.Type)));
    }
}
