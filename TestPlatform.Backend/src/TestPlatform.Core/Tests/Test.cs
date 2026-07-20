using CSharpFunctionalExtensions;
using TestPlatform.Core.Shared;

namespace TestPlatform.Core.Tests;

public class Test
{
    private const int MaxQuestions = 50;
    private const int MaxLengthTitle = 200;
    private const int MaxLengthDescription = 250;
    private const int MinTimeLimitSeconds = 100;
    private const int MaxTimeLimitSeconds = 14100;

    private readonly List<QuestionAssignment> _questions = new();

    private Test() { }

    private Test(
        Guid id,
        string title,
        string description,
        Guid authorId)
    {
        Id = id;
        Title = title;
        Description = description;
        AuthorId = authorId;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public int? TimeLimitSeconds { get; private set; }

    public Guid? CoverImageId { get; private set; }

    public Guid AuthorId { get; }

    public DateTime CreatedAt { get; }

    public IReadOnlyCollection<QuestionAssignment> Questions => _questions.AsReadOnly();

    private int TotalQuestions => _questions.Count;

    public static Result<Test> Create(
        string title,
        string description,
        Guid authorId)
    {
        var validation = Validate(title, description);
        if (validation.IsFailure)
            return Result.Failure<Test>(validation.Error);

        return Result.Success(
            new Test(
                Guid.NewGuid(),
                title,
                description,
                authorId));
    }

    public Result AddQuestion(Guid questionId, int score)
    {
        if (_questions.Any(x => x.QuestionId == questionId))
            return Result.Failure("Вопрос уже добавлен");

        if (_questions.Count >= MaxQuestions)
            return Result.Failure($"Максимум {MaxQuestions} вопросов");

        _questions.Add(new QuestionAssignment(
            questionId,
            _questions.Count + 1,
            score));

        return Result.Success();
    }

    public Result RemoveQuestion(Guid questionId)
    {
        var question = _questions
            .FirstOrDefault(x => x.QuestionId == questionId);
        if (question is null)
            return Result.Failure("Вопрос не найден");

        _questions.Remove(question);

        Reorder();

        return Result.Success();
    }

    public Result ChangeTitle(string title)
    {
        var validation = ValidateTitle(title);
        if (validation.IsFailure)
            return validation;

        Title = title;

        return Result.Success();
    }

    public Result ChangeDescription(string description)
    {
        var validation = ValidateDescription(description);
        if (validation.IsFailure)
            return validation;

        Description = description;

        return Result.Success();
    }

    public Result ChangeTimeLimit(int? seconds)
    {
        if (seconds.HasValue &&
            (seconds < MinTimeLimitSeconds ||
             seconds > MaxTimeLimitSeconds))
        {
            return Result.Failure(
                $"'{nameof(seconds)}' должно быть от {MinTimeLimitSeconds} до {MaxTimeLimitSeconds} секунд.");
        }

        TimeLimitSeconds = seconds;

        return Result.Success();
    }

    public Result RemoveTimeLimit()
    {
        TimeLimitSeconds = null;

        return Result.Success();
    }

    public Result ChangeCoverImage(Guid fileAssetId)
    {
        CoverImageId = fileAssetId;

        return Result.Success();
    }

    public Result RemoveCoverImage()
    {
        CoverImageId = null;

        return Result.Success();
    }

    private static Result Validate(
        string title,
        string description)
    {
        var titleResult = ValidateTitle(title);
        if (titleResult.IsFailure)
            return Result.Failure(titleResult.Error);

        var descriptionResult = ValidateDescription(description);
        if (descriptionResult.IsFailure)
            return Result.Failure(descriptionResult.Error);

        return Result.Success();
    }

    private static Result ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > MaxLengthTitle)
            return Result.Failure($"'{nameof(title)}' не может быть null или пустым, длиннее {MaxLengthTitle} символов.");

        return Result.Success();
    }

    private static Result ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Length > MaxLengthDescription)
            return Result.Failure("Invalid description");

        return Result.Success();
    }

    private void Reorder()
    {
        var ordered = _questions
            .OrderBy(x => x.Order)
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
            ordered[i].SetOrder(i + 1);
    }
}