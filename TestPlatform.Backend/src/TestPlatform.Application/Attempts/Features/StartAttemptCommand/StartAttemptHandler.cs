using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Tags.DTOs;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Features.StartAttemptCommand;

public record StartAttemptCommand(StartRequest Request) : ICommand;

public class StartAttemptHandler : ICommandHandler<StartResponse, StartAttemptCommand>
{
    private readonly IAttemptsRepository _attemptsRepository;
    private readonly IAttemptSourceService _attemptSourceService;
    private readonly ILogger<StartAttemptHandler> _logger;

    public StartAttemptHandler(
        IAttemptsRepository attemptsRepository,
        IAttemptSourceService attemptSourceService,
        ILogger<StartAttemptHandler> logger)
    {
        _attemptsRepository = attemptsRepository;
        _attemptSourceService = attemptSourceService;
        _logger = logger;
    }

    public async Task<Result<StartResponse>> Handle(StartAttemptCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var sourceResult = await _attemptSourceService.GetSourceAsync(request.Type, request.SourceId, cancellationToken);

        if (sourceResult.IsFailure)
        {
            _logger.LogWarning("Source {Type} with id {Id} not found", request.Type, request.SourceId);
            return Result.Failure<StartResponse>(sourceResult.Error);
        }

        var source = sourceResult.Value;

        var attemptResult = Attempt.Create(
            source.Questions.Count,
            source.Questions.Sum(q => q.Points),
            request.UserId,
            (AttemptType)request.Type,
            request.SourceId);

        if (attemptResult.IsFailure)
            return Result.Failure<StartResponse>(attemptResult.Error);

        var attempt = attemptResult.Value;

        var startResult = attempt.Start();
        if (startResult.IsFailure)
            return Result.Failure<StartResponse>(startResult.Error);

        var saveResult = await _attemptsRepository.AddAsync(attempt, cancellationToken);
        if (saveResult.IsFailure)
            return Result.Failure<StartResponse>(saveResult.Error);

        var sourceDto = new AttemptSourceResponse(
            source.Questions.Count,
            source.TimeLimitSeconds,
            request.Type,
            source.Questions.Select(q => new QuestionResponse(
                q.Id,
                q.Text,
                q.QuestionTypeId,
                q.Points,
                q.ImageName,
                q.Tags.Select(z => new TagResponse(
                    z.Id,
                    z.Name,
                    z.Description)).ToList(),
                q.AnswerOptions.Select(a => new AnswerOptionResponse(
                    a.Id,
                    a.Text,
                    null,
                    a.ImageName)).ToList())).ToList());

        var response = new StartResponse(attempt.Id, sourceDto);

        return Result.Success(response);
    }
}