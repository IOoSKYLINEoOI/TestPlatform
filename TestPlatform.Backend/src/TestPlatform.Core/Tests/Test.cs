using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Tests;

public class Test
{
    private const int MaxQuestions = 50;
    private const int MaxLengthName = 100;
    private const int MaxLengthDescription = 250;
    private const int MinTimeLimitSeconds = 100;
    private const int MaxTimeLimitSeconds = 14100;

    private readonly List<Guid> _questionsIds = new();

    private Test(Guid id, string name, int? timeLimitSeconds, string description, Guid? authorId, string? coverImageName)
    {
        Id = id;
        Name = name;
        TimeLimitSeconds = timeLimitSeconds;
        Description = description;
        AuthorId = authorId;
        CoverImageName = coverImageName;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Description { get; }

    public int? TimeLimitSeconds { get; }

    public string? CoverImageName { get; }

    public Guid? AuthorId { get; }

    public IReadOnlyCollection<Guid> QuestionsIds => _questionsIds.AsReadOnly();

    private int TotalQuestions => _questionsIds.Count;

    public static Result<Test> Create(
        string name,
        int? timeLimitSeconds,
        string description,
        Guid? authorId,
        string? coverImageUrl)
    {
        var validation = Validate(name, timeLimitSeconds, description, authorId);
        if (validation.IsFailure)
            return Result.Failure<Test>(validation.Error);

        return Result.Success(new Test(Guid.NewGuid(), name, timeLimitSeconds, description, authorId, coverImageUrl));
    }

    public static Result<Test> CreateWithId(
        Guid id,
        string name,
        int? timeLimitSeconds,
        string description,
        Guid? authorId,
        string? coverImageUrl)
    {
        var validation = Validate(name, timeLimitSeconds, description, authorId);
        if (validation.IsFailure)
            return Result.Failure<Test>(validation.Error);

        return Result.Success(new Test(id, name, timeLimitSeconds, description, authorId, coverImageUrl));
    }

    public Result AddQuestion(Guid questionId)
    {
        if (TotalQuestions >= MaxQuestions)
            return Result.Failure($"Нельзя добавить больше {MaxQuestions} вопросов.");

        _questionsIds.Add(questionId);
        return Result.Success();
    }

    private static Result Validate(
        string name,
        int? timeLimitSeconds,
        string description,
        Guid? authorId)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxLengthName)
            return Result.Failure<Test>($"'{nameof(name)}' не может быть null или пустым, длиннее {MaxLengthName} символов.");
        if (string.IsNullOrWhiteSpace(description) || description.Length > MaxLengthDescription)
            return Result.Failure<Test>($"'{nameof(description)}' не может быть null или пустым, длиннее {MaxLengthDescription} символов.");
        if (timeLimitSeconds is < MinTimeLimitSeconds or > MaxTimeLimitSeconds)
        {
            return Result.Failure<Test>(
                $"'{nameof(timeLimitSeconds)}' должно быть от {MinTimeLimitSeconds} до {MaxTimeLimitSeconds} секунд.");
        }

        if (authorId == Guid.Empty)
            return Result.Failure<Test>("Автор теста не задан.");

        return Result.Success();
    }
}