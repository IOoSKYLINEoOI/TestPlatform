using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Models.Test;

public class Test
{
    private const int MaxQuestions = 50;
    private const int MaxLengthName = 100;
    private const int MaxLengthDescription = 250;
    private const int MinTimeLimitSeconds = 100;
    private const int MaxTimeLimitSeconds = 14100;

    private readonly List<Question> _questions = new();
    private readonly List<Guid> _categories = new();

    private Test(Guid id, string name, int? timeLimitSeconds, string description, Guid authorId, string? coverImageUrl)
    {
        Id = id;
        Name = name;
        TimeLimitSeconds = timeLimitSeconds;
        Description = description;
        AuthorId = authorId;
        CoverImageUrl = coverImageUrl;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int? TimeLimitSeconds { get; }
    public string? CoverImageUrl { get; }
    
    public Guid AuthorId { get; }
    public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();
    private int TotalQuestions => _questions.Count;
    public IReadOnlyCollection<Guid> Categories => _categories.AsReadOnly();


    public static Result<Test> Create(
        string name, 
        int? timeLimitSeconds, 
        string description, 
        Guid userId, 
        string? coverImageUrl)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxLengthName)
            return Result.Failure<Test>($"'{nameof(name)}' не может быть null или пустым, длиннее {MaxLengthName} символов.");
        if (string.IsNullOrWhiteSpace(description) || description.Length > MaxLengthDescription)
            return Result.Failure<Test>(
                $"'{nameof(description)}' не может быть null или пустым, длиннее {MaxLengthDescription} символов.");
        if (timeLimitSeconds is < MinTimeLimitSeconds or > MaxTimeLimitSeconds)
            return Result.Failure<Test>(
                $"'{nameof(timeLimitSeconds)}' должно быть от {MinTimeLimitSeconds} до {MaxTimeLimitSeconds} секунд.");
        if(userId == Guid.Empty)
            return Result.Failure<Test>("Автор теста не задан.");

        return Result.Success(new Test(Guid.NewGuid(), name, timeLimitSeconds, description, userId, coverImageUrl));
    }

    public static Test FromPersistence(
        Guid id, 
        string name, 
        int? timeLimitSeconds, 
        string description, 
        Guid userId, 
        string? coverImageUrl, 
        IEnumerable<Question> questions, 
        IEnumerable<Guid> categories)
    {
        var test = new Test(id, name, timeLimitSeconds, description, userId, coverImageUrl);
        
        test._questions.AddRange(questions);

        test.AddCategories(categories);

        return test;
    }

    public Result AddQuestion(Question question)
    {
        if (TotalQuestions >= MaxQuestions)
            return Result.Failure($"Нельзя добавить больше {MaxQuestions} вопросов.");

        _questions.Add(question);
        return Result.Success();
    }

    private Result AddCategories(IEnumerable<Guid> categoryIds)
    {
        foreach (var id in categoryIds)
        {
            if (_categories.Contains(id))
                continue;

            _categories.Add(id);
        }

        return Result.Success();
    }
}