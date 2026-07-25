using CSharpFunctionalExtensions;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Core.Attempts;

public class Attempt
{
    private readonly List<AttemptAnswer> _attemptAnswers = new();
    private readonly List<AttemptQuestionSelection> _questionSelections = new();

    private Attempt() { }

    private Attempt(
        Guid id,
        Guid userId,
        Guid requestId,
        AttemptType type,
        Guid sourceId,
        IReadOnlyCollection<AttemptQuestionSelection> questionSelections,
        int? timeLimitSeconds,
        decimal? minPassingScore,
        double? minPassingPercent,
        DateTime? latestFinishAt,
        DateTime? reviewAvailableAt)
    {
        Id = id;
        UserId = userId;
        RequestId = requestId;
        Type = type;
        SourceId = sourceId;
        _questionSelections.AddRange(questionSelections.OrderBy(x => x.Order));
        TotalQuestions = questionSelections.Count;
        TotalMaxScore = questionSelections.Sum(x => x.Score);
        Status = AttemptStatus.NOT_STARTED;
        TimeLimitSeconds = timeLimitSeconds;
        MinPassingScore = minPassingScore;
        MinPassingPercent = minPassingPercent;
        LatestFinishAt = latestFinishAt;
        ReviewAvailableAt = reviewAvailableAt;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public Guid RequestId { get; }

    public AttemptType Type { get; }

    public Guid SourceId { get; }

    public int AttemptNumber { get; private set; }

    public int TotalQuestions { get; }

    public decimal TotalMaxScore { get; }

    public int? TimeLimitSeconds { get; }

    public decimal? MinPassingScore { get; }

    public double? MinPassingPercent { get; }

    public DateTime? LatestFinishAt { get; }

    public DateTime? ReviewAvailableAt { get; }

    public DateTime? Deadline { get; private set; }

    public AttemptStatus Status { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? FinishedAt { get; private set; }

    public AttemptResult? AttemptResult { get; private set; }

    public Score Score => AttemptResult is null
            ? new Score(0, TotalMaxScore)
            : new Score(AttemptResult.EarnedPoints, TotalMaxScore);

    public IReadOnlyCollection<AttemptAnswer> AttemptAnswers => _attemptAnswers.AsReadOnly();

    public IReadOnlyCollection<AttemptQuestionSelection> QuestionSelections => _questionSelections.AsReadOnly();


    public static Result<Attempt> Create(
        Guid userId,
        AttemptType type,
        Guid sourceId,
        IReadOnlyCollection<AttemptQuestionSelection> questionSelections,
        int? timeLimitSeconds,
        decimal? minPassingScore = null,
        double? minPassingPercent = null,
        DateTime? latestFinishAt = null,
        DateTime? reviewAvailableAt = null,
        Guid? requestId = null)
    {
        if (userId == Guid.Empty || sourceId == Guid.Empty)
        {
            return Result.Failure<Attempt>("attempt.identity_required");
        }

        if (requestId == Guid.Empty)
        {
            return Result.Failure<Attempt>("attempt.request_id_required");
        }

        if (questionSelections.Count == 0)
        {
            return Result.Failure<Attempt>("attempt.questions_required");
        }

        if (questionSelections.Select(x => x.QuestionId).Distinct().Count() != questionSelections.Count)
        {
            return Result.Failure<Attempt>("attempt.duplicate_questions");
        }

        if (questionSelections.Any(x => x.Score <= 0))
        {
            return Result.Failure<Attempt>("attempt.invalid_question_score");
        }

        var orders = questionSelections.Select(x => x.Order).OrderBy(x => x).ToArray();
        if (!orders.SequenceEqual(Enumerable.Range(1, questionSelections.Count)))
        {
            return Result.Failure<Attempt>("attempt.invalid_question_order");
        }

        if (timeLimitSeconds is <= 0)
        {
            return Result.Failure<Attempt>("attempt.invalid_time_limit");
        }

        if (type == AttemptType.Test && (minPassingScore.HasValue || minPassingPercent.HasValue))
        {
            return Result.Failure<Attempt>("attempt.test_cannot_have_passing_rule");
        }

        if (type == AttemptType.Exam && minPassingScore.HasValue == minPassingPercent.HasValue)
        {
            return Result.Failure<Attempt>("attempt.exam_requires_one_passing_rule");
        }

        return Result.Success(
            new Attempt(
                 Guid.NewGuid(),
                 userId,
                 requestId ?? Guid.NewGuid(),
                 type,
                 sourceId,
                 questionSelections,
                 timeLimitSeconds,
                 minPassingScore,
                 minPassingPercent,
                 latestFinishAt,
                 reviewAvailableAt));
    }

    public Result Start()
    {
        if (Status != AttemptStatus.NOT_STARTED)
        {
            return Result.Failure("attempt.already_started");
        }

        StartedAt = DateTime.UtcNow;

        if (LatestFinishAt.HasValue && LatestFinishAt.Value <= StartedAt.Value)
        {
            StartedAt = null;
            return Result.Failure("attempt.source_closed");
        }

        Status = AttemptStatus.STARTED;

        if (TimeLimitSeconds.HasValue)
        {
            Deadline = StartedAt.Value.AddSeconds(TimeLimitSeconds.Value);
        }

        if (LatestFinishAt.HasValue && (!Deadline.HasValue || LatestFinishAt.Value < Deadline.Value))
        {
            Deadline = LatestFinishAt.Value;
        }

        return Result.Success();
    }

    public Result AssignAttemptNumber(int attemptNumber)
    {
        if (AttemptNumber != 0 || attemptNumber <= 0)
        {
            return Result.Failure("attempt.invalid_number");
        }

        AttemptNumber = attemptNumber;
        return Result.Success();
    }

    public Result Finish(
        IReadOnlyCollection<AttemptQuestion> questions)
    {
        if (Status != AttemptStatus.STARTED)
        {
            return Result.Failure("attempt.cannot_finish");
        }

        var notExpired = CheckNotExpired();
        if (notExpired.IsFailure)
        {
            return notExpired;
        }

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
                return Result.Failure("attempt.question_not_found");
            }

            decimal normalizedScore =
                question.Question
                    .AnswerDefinition
                    .Evaluate(answer.ToEvaluationValue());

            earnedPoints +=
                normalizedScore * question.Score;

            if (normalizedScore >= 1m)
            {
                correctAnswers++;
            }
        }

        var percentage = TotalMaxScore == 0
            ? 0
            : (double)(earnedPoints / TotalMaxScore * 100);
        bool? passed = Type == AttemptType.Exam
            ? (!MinPassingScore.HasValue || earnedPoints >= MinPassingScore.Value)
                && (!MinPassingPercent.HasValue || percentage >= MinPassingPercent.Value)
            : null;

        AttemptResult = new AttemptResult(correctAnswers, earnedPoints, passed);

        Status = AttemptStatus.FINISHED;
        FinishedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result SaveAnswer(AttemptAnswer answer)
    {
        var notExpired = CheckNotExpired();
        if (notExpired.IsFailure)
        {
            return notExpired;
        }

        if (Status != AttemptStatus.STARTED)
        {
            return Result.Failure("attempt.answer.cannot_save");
        }

        if (_questionSelections.All(x => x.QuestionId != answer.QuestionId))
        {
            return Result.Failure("attempt.question_not_in_attempt");
        }

        _attemptAnswers.RemoveAll(x => x.QuestionId == answer.QuestionId);
        _attemptAnswers.Add(answer);

        return Result.Success();
    }

    public Result RemoveAnswer(Guid questionId)
    {
        var notExpired = CheckNotExpired();
        if (notExpired.IsFailure)
        {
            return notExpired;
        }

        if (Status != AttemptStatus.STARTED)
        {
            return Result.Failure("attempt.answer.cannot_remove");
        }

        _attemptAnswers.RemoveAll(x => x.QuestionId == questionId);
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

    public Result Cancel()
    {
        if (Status == AttemptStatus.CANCELLED)
        {
            return Result.Failure("attempt.already_cancelled");
        }

        Status = AttemptStatus.CANCELLED;
        FinishedAt ??= DateTime.UtcNow;
        return Result.Success();
    }

    private bool IsExpired()
    {
        if (!Deadline.HasValue)
        {
            return false;
        }

        return DateTime.UtcNow > Deadline.Value;
    }

    private Result CheckNotExpired()
    {
        if (!IsExpired())
        {
            return Result.Success();
        }

        Expire();

        return Result.Failure("attempt.expired");
    }

    private Result SetFinalStatus(AttemptStatus status)
    {
        if (Status is AttemptStatus.FINISHED
            or AttemptStatus.EXPIRED
            or AttemptStatus.ABANDONED
            or AttemptStatus.CANCELLED)
        {
            return Result.Failure("attempt.already_finished");
        }

        Status = status;
        FinishedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
