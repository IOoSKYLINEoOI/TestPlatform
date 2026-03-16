using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Attempts.Features.FinishAttemptCommand;

public record FinishAttemptCommand(Guid Id, FinishRequest FinishRequest) : ICommand;

public class FinishAttemptHandler : ICommandHandler<AttemptResponse, FinishAttemptCommand>
{
    private readonly IAttemptsRepository _attemptsRepository;
    private readonly IAttemptsReadRepository _attemptsReadRepository;
    private readonly IAttemptSourceService _attemptSourceService;
    private readonly IQuestionCheckerFactory _checkerFactory;
    private readonly ILogger<FinishAttemptHandler> _logger;

    public FinishAttemptHandler(
        IAttemptsRepository attemptsRepository,
        IAttemptsReadRepository attemptsReadRepository,
        IAttemptSourceService attemptSourceService,
        IQuestionCheckerFactory checkerFactory,
        ILogger<FinishAttemptHandler> logger)
    {
        _attemptsRepository = attemptsRepository;
        _attemptsReadRepository = attemptsReadRepository;
        _attemptSourceService = attemptSourceService;
        _checkerFactory = checkerFactory;
        _logger = logger;
    }

    public async Task<Result<AttemptResponse>> Handle(FinishAttemptCommand command, CancellationToken cancellationToken)
    {
         var attemptEntity = await _attemptsReadRepository.ReadAttemptByIdAsync(command.Id, cancellationToken);
         if (attemptEntity is null)
             return Result.Failure<AttemptResponse>("Attempt not found");

         var attempt = Attempt.FromPersistence(
            attemptEntity.Id,
            attemptEntity.TotalQuestions,
            attemptEntity.MaxPoints,
            attemptEntity.EarnedPoints,
            attemptEntity.CorrectAnswers,
            attemptEntity.UserId,
            (AttemptStatus)attemptEntity.Status,
            attemptEntity.StartedAt,
            attemptEntity.FinishedAt,
            (AttemptType)attemptEntity.Type,
            attemptEntity.SourceId);

         if (attempt.FinishedAt != null)
             return Result.Failure<AttemptResponse>("Тест уже завершён");

         if (attempt.StartedAt == null)
             return Result.Failure<AttemptResponse>("Попытка ещё не была начата");

         var sourceResult = await _attemptSourceService.GetSourceAsync(
             (AttemptTypeDto)attempt.Type,
             attempt.SourceId,
             cancellationToken);

         if (sourceResult.IsFailure)
             return Result.Failure<AttemptResponse>("Источник попытки не найден");

         var source = sourceResult.Value;

         bool isTimeExpired =
            source.TimeLimitSeconds.HasValue &&
            DateTime.UtcNow >
            attempt.StartedAt.Value.AddSeconds(source.TimeLimitSeconds.Value);

         int correctCount = 0;
         decimal earnedPoints = 0;

         if (!isTimeExpired)
         {
             var checkResult = CheckAnswers(source, command.FinishRequest.UserAnswers);
             if (checkResult.IsFailure)
                 return Result.Failure<AttemptResponse>(checkResult.Error);

             correctCount = checkResult.Value.correctAnswers;
             earnedPoints = checkResult.Value.points;
         }

         var finishResult = attempt.Finish(correctCount, earnedPoints);
         if (finishResult.IsFailure)
             return Result.Failure<AttemptResponse>(finishResult.Error);

         var updateResult = await _attemptsRepository.UpdateAsync(attempt, cancellationToken);
         if (updateResult.IsFailure)
             return Result.Failure<AttemptResponse>(updateResult.Error);

         var response = new AttemptResponse(
             attempt.Id,
             attempt.TotalQuestions,
             attempt.CorrectAnswers,
             attempt.EarnedPoints,
             attempt.MaxPoints,
             attempt.UserId,
             attempt.StartedAt,
             attempt.FinishedAt,
             (AttemptStatusDto)attempt.Status,
             (AttemptTypeDto)attempt.Type,
             attempt.SourceId);

         return Result.Success(response);
    }

    private Result<(int correctAnswers, decimal points)> CheckAnswers(
        IAttemptSource source,
        IReadOnlyList<UserAnswer> userAnswers)
    {
        int correct = 0;
        decimal points = 0;

        foreach (var userAnswer in userAnswers)
        {
            var question = source.Questions.FirstOrDefault(q => q.Id == userAnswer.QuestionId);
            if (question is null)
                return Result.Failure<(int, decimal)>($"Вопрос {userAnswer.QuestionId} не найден");

            var questionType = (QuestionType)question.QuestionTypeId;

            var checker = _checkerFactory.GetChecker(questionType);

            var result = checker.Check(question, userAnswer);

            if (result.IsFailure)
                return Result.Failure<(int, decimal)>(result.Error);

            if (result.Value)
            {
                correct++;
                points += question.Points;
            }
        }

        return Result.Success((correct, points));
    }

}