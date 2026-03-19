using CSharpFunctionalExtensions;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Core.Attempts;

public class Attempt
{
    private Attempt(Guid id, int totalQuestions, decimal maxPoints, Guid userId, AttemptType type, Guid sourceId)
    {
        Id = id;
        TotalQuestions = totalQuestions;
        MaxPoints = maxPoints;
        EarnedPoints = 0;
        UserId = userId;
        Type = type;
        SourceId = sourceId;
        Status = AttemptStatus.NOT_STARTED;
    }

    public Guid Id { get; }

    public int TotalQuestions { get; }

    public int? CorrectAnswers { get; private set; }

    public decimal EarnedPoints { get; private set; }

    public decimal MaxPoints { get; }

    public AttemptStatus Status { get; private set; }

    public Guid UserId { get; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? FinishedAt { get; private set; }

    public AttemptType Type { get; }

    public Guid SourceId { get; }

    public decimal Score => MaxPoints > 0 ? EarnedPoints / MaxPoints : 0m;

    public static Result<Attempt> Create(int totalQuestions, decimal maxPoints, Guid userId, AttemptType type, Guid sourceId)
    {
        if (totalQuestions <= 0)
            return Result.Failure<Attempt>("Количество вопросов должно быть больше 0.");

        return Result.Success(new Attempt(Guid.NewGuid(), totalQuestions, maxPoints, userId, type, sourceId));
    }

    public static Attempt FromPersistence(
        Guid id,
        int totalQuestions,
        decimal maxPoints,
        decimal earnedPoints,
        int? correctAnswers,
        Guid userId,
        AttemptStatus status,
        DateTime? startedAt,
        DateTime? finishedAt,
        AttemptType type,
        Guid sourceId)
    {
        var attempt = new Attempt(id, totalQuestions, maxPoints, userId, type, sourceId);

        attempt.EarnedPoints = earnedPoints;
        attempt.CorrectAnswers = correctAnswers;
        attempt.Status = status;
        attempt.StartedAt = startedAt;
        attempt.FinishedAt = finishedAt;

        return attempt;
    }

    public Result Start()
    {
        if (Status != AttemptStatus.NOT_STARTED)
            return Result.Failure("Попытка уже была начата.");

        StartedAt = DateTime.UtcNow;
        Status = AttemptStatus.STARTED;
        return Result.Success();
    }

    public Result Finish(int correctAnswers, decimal earnedPoints)
    {
        if (Status != AttemptStatus.STARTED)
            return Result.Failure("Попытка не может быть завершена.");

        if (correctAnswers < 0 || correctAnswers > TotalQuestions)
            return Result.Failure("Некорректное количество правильных ответов.");

        if (earnedPoints < 0 || earnedPoints > MaxPoints)
            return Result.Failure("Некорректное количество набранных очков.");

        CorrectAnswers = correctAnswers;
        EarnedPoints = earnedPoints;
        Status = AttemptStatus.FINISHED;
        FinishedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Expire() => SetFinalStatus(AttemptStatus.EXPIRED);

    public Result Abandon() => SetFinalStatus(AttemptStatus.ABANDONED);

    public Result Cancel() => SetFinalStatus(AttemptStatus.CANCELLED);

    private Result SetFinalStatus(AttemptStatus status)
    {
        if (Status == AttemptStatus.FINISHED ||
            Status == AttemptStatus.EXPIRED ||
            Status == AttemptStatus.ABANDONED ||
            Status == AttemptStatus.CANCELLED)
            return Result.Failure("Попытка уже завершена.");

        Status = status;
        FinishedAt = DateTime.UtcNow;
        return Result.Success();
    }
}