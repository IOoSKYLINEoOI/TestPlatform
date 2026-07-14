using CSharpFunctionalExtensions;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Core.Attempts;

public class Attempt
{
    private readonly List<AttemptAnswer> _attemptAnswers = new();

    private Attempt() { }

    private Attempt(
        Guid id,
        Guid userId,
        AttemptType type,
        Guid sourceId,
        int totalQuestions,
        decimal totalMaxScore,
        int? timeLimitSeconds)
    {
        Id = id;
        UserId = userId;
        Type = type;
        SourceId = sourceId;
        TotalQuestions = totalQuestions;
        TotalMaxScore = totalMaxScore;
        Status = AttemptStatus.NOT_STARTED;
        TimeLimitSeconds = timeLimitSeconds;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public AttemptType Type { get; }

    public Guid SourceId { get; }

    public int TotalQuestions { get; }

    public decimal TotalMaxScore { get; }

    public int? TimeLimitSeconds { get; }

    public DateTime? Deadline { get; private set; }

    public AttemptStatus Status { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? FinishedAt { get; private set; }

    public AttemptResult? AttemptResult { get; private set; }

    public Score Score => AttemptResult is null
            ? new Score(0, TotalMaxScore)
            : new Score(AttemptResult.EarnedPoints, TotalMaxScore);

    public IReadOnlyCollection<AttemptAnswer> AttemptAnswers => _attemptAnswers.AsReadOnly();


    public static Result<Attempt> Create(
        Guid userId,
        AttemptType type,
        Guid sourceId,
        int totalQuestions,
        decimal totalMaxScore,
        int? timeLimitSeconds)
    {
        if (totalQuestions <= 0)
            return Result.Failure<Attempt>("Количество вопросов должно быть больше 0");

        return Result.Success(
            new Attempt(
                 Guid.NewGuid(),
                 userId,
                 type,
                 sourceId,
                 totalQuestions,
                 totalMaxScore,
                 timeLimitSeconds));
    }

    public Result Start()
    {
        if (Status != AttemptStatus.NOT_STARTED)
            return Result.Failure("Попытка уже была начата.");

        StartedAt = DateTime.UtcNow;
        Status = AttemptStatus.STARTED;

        if (TimeLimitSeconds.HasValue)
        {
            Deadline = StartedAt.Value.AddSeconds(TimeLimitSeconds.Value);
        }

        return Result.Success();
    }

    public Result Finish(
        IReadOnlyCollection<AttemptQuestion> questions)
    {
        if (Status != AttemptStatus.STARTED)
            return Result.Failure("Попытка не может быть завершена.");

        var notExpired = CheckNotExpired();
        if (notExpired.IsFailure)
            return notExpired;

        var questionMap = questions.ToDictionary(
            x => x.Question.Id);

        decimal earnedPoints = 0;
        int correctAnswers = 0;

        foreach (var answer in _attemptAnswers)
        {
            if (!questionMap.TryGetValue(
                    answer.QuestionId,
                    out var question))
            {
                return Result.Failure(
                    $"Вопрос {answer.QuestionId} не найден.");
            }

            decimal normalizedScore =
                question.Question
                    .AnswerDefinition
                    .Evaluate(answer);

            earnedPoints +=
                normalizedScore * question.Score;

            if (normalizedScore >= 1m)
                correctAnswers++;
        }

        AttemptResult = new AttemptResult(
            correctAnswers,
            earnedPoints);

        Status = AttemptStatus.FINISHED;
        FinishedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result SaveAnswer(AttemptAnswer answer)
    {
        var notExpired = CheckNotExpired();
        if (notExpired.IsFailure)
            return notExpired;

        if (Status != AttemptStatus.STARTED)
            return Result.Failure("Ответ невозможно добавить.");

        _attemptAnswers.RemoveAll(x => x.QuestionId == answer.QuestionId);
        _attemptAnswers.Add(answer);

        return Result.Success();
    }

    public AttemptAnswer? GetAnswer(Guid questionId)
        => _attemptAnswers.FirstOrDefault(a => a.QuestionId == questionId);

    public bool HasAnswered(Guid questionId)
        => _attemptAnswers.Any(a => a.QuestionId == questionId);

    public int AnsweredQuestions => _attemptAnswers.Count;

    public bool IsCompleted => _attemptAnswers.Count == TotalQuestions;

    public Result Expire() => SetFinalStatus(AttemptStatus.EXPIRED);

    public Result Abandon() => SetFinalStatus(AttemptStatus.ABANDONED);

    public Result Cancel() => SetFinalStatus(AttemptStatus.CANCELLED);

    private bool IsExpired()
    {
        if (!Deadline.HasValue)
            return false;

        return DateTime.UtcNow > Deadline.Value;
    }

    private Result CheckNotExpired()
    {
        if (!IsExpired())
            return Result.Success();

        Expire();

        return Result.Failure("Время попытки истекло.");
    }

    private Result SetFinalStatus(AttemptStatus status)
    {
        if (Status is AttemptStatus.FINISHED
            or AttemptStatus.EXPIRED
            or AttemptStatus.ABANDONED
            or AttemptStatus.CANCELLED)
            return Result.Failure("Попытка уже завершена.");

        Status = status;
        FinishedAt = DateTime.UtcNow;
        return Result.Success();
    }
}