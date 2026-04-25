using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Tests;

public class Test
{
    private const int MaxQuestions = 50;
    private const int MaxLengthName = 100;
    private const int MaxLengthDescription = 250;
    private const int MinTimeLimitSeconds = 100;
    private const int MaxTimeLimitSeconds = 14100;

    private readonly List<Guid> _questionIds = new();

    private Test(Guid id, string title, int? timeLimitSeconds, string description, Guid authorId, string? coverImageName)
    {
        Id = id;
        Title = title;
        TimeLimitSeconds = timeLimitSeconds;
        Description = description;
        AuthorId = authorId;
        CoverImageName = coverImageName;
    }

    public Guid Id { get; }

    public string Title { get; }

    public string Description { get; }

    public int? TimeLimitSeconds { get; }

    public string? CoverImageName { get; }

    public Guid AuthorId { get; }

    public IReadOnlyCollection<Guid> QuestionIds => _questionIds;

    private int TotalQuestions => _questionIds.Count;

    public static Result<Test> Create(
        string name,
        int? timeLimitSeconds,
        string description,
        Guid authorId,
        string? coverImageUrl)
    {
        var validation = Validate(name, timeLimitSeconds, description);
        if (validation.IsFailure)
            return Result.Failure<Test>(validation.Error);

        return Result.Success(new Test(Guid.NewGuid(), name, timeLimitSeconds, description, authorId, coverImageUrl));
    }

    public static Result<Test> CreateWithId(
        Guid id,
        string name,
        int? timeLimitSeconds,
        string description,
        Guid authorId,
        string? coverImageUrl)
    {
        var validation = Validate(name, timeLimitSeconds, description);
        if (validation.IsFailure)
            return Result.Failure<Test>(validation.Error);

        return Result.Success(new Test(id, name, timeLimitSeconds, description, authorId, coverImageUrl));
    }

    public Result AddQuestion(Guid questionId)
    {
        if (TotalQuestions >= MaxQuestions)
            return Result.Failure($"Нельзя добавить больше {MaxQuestions} вопросов.");

        _questionIds.Add(questionId);
        return Result.Success();
    }

    private static Result Validate(
        string title,
        int? timeLimitSeconds,
        string description)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > MaxLengthName)
            return Result.Failure($"'{nameof(title)}' не может быть null или пустым, длиннее {MaxLengthName} символов.");

        if (string.IsNullOrWhiteSpace(description) || description.Length > MaxLengthDescription)
            return Result.Failure($"'{nameof(description)}' не может быть null или пустым, длиннее {MaxLengthDescription} символов.");

        if (timeLimitSeconds.HasValue &&
            (timeLimitSeconds < MinTimeLimitSeconds ||
             timeLimitSeconds > MaxTimeLimitSeconds))
        {
            return Result.Failure(
                $"'{nameof(timeLimitSeconds)}' должно быть от {MinTimeLimitSeconds} до {MaxTimeLimitSeconds} секунд.");
        }

        return Result.Success();
    }
}