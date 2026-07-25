using CSharpFunctionalExtensions;
using TestPlatform.Core.Tests.Enums;

namespace TestPlatform.Core.Tests;

public class Test
{
    private const int MaxQuestions = 50;
    private const int MaxLengthTitle = 200;
    private const int MaxLengthDescription = 250;
    private const int MinTimeLimitSeconds = 100;
    private const int MaxTimeLimitSeconds = 14100;

    private readonly List<TestQuestion> _questions = new();

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
        UpdatedAt = CreatedAt;
        Status = TestStatus.Draft;
    }

    public Guid Id { get; }

    public string Title { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public int? TimeLimitSeconds { get; private set; }

    public Guid? CoverImageId { get; private set; }

    public Guid AuthorId { get; }

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? PublishedAt { get; private set; }

    public TestStatus Status { get; private set; }

    public IReadOnlyCollection<TestQuestion> Questions => _questions.AsReadOnly();

    private int TotalQuestions => _questions.Count;

    public static Result<Test> Create(
        string title,
        string description,
        Guid authorId)
    {
        var validation = Validate(title, description);
        if (validation.IsFailure)
        {
            return Result.Failure<Test>(validation.Error);
        }

        if (authorId == Guid.Empty)
        {
            return Result.Failure<Test>("test.author_required");
        }

        return Result.Success(
            new Test(
                Guid.NewGuid(),
                title.Trim(),
                description.Trim(),
                authorId));
    }

    public Result AddQuestion(Guid questionId)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        if (_questions.Any(x => x.QuestionId == questionId))
        {
            return Result.Failure("test.question_already_added");
        }

        if (_questions.Count >= MaxQuestions)
        {
            return Result.Failure("test.questions_limit_reached");
        }

        _questions.Add(new TestQuestion(questionId, _questions.Count + 1));

        Touch();

        return Result.Success();
    }

    public Result RemoveQuestion(Guid questionId)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        var question = _questions
            .FirstOrDefault(x => x.QuestionId == questionId);
        if (question is null)
        {
            return Result.Failure("test.question_not_found");
        }

        _questions.Remove(question);

        Reorder();
        Touch();

        return Result.Success();
    }

    public Result ChangeTitle(string title)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        var validation = ValidateTitle(title);
        if (validation.IsFailure)
        {
            return validation;
        }

        Title = title.Trim();
        Touch();

        return Result.Success();
    }

    public Result ChangeDescription(string description)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        var validation = ValidateDescription(description);
        if (validation.IsFailure)
        {
            return validation;
        }

        Description = description.Trim();
        Touch();

        return Result.Success();
    }

    public Result ChangeTimeLimit(int? seconds)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        if (seconds.HasValue &&
            (seconds < MinTimeLimitSeconds ||
             seconds > MaxTimeLimitSeconds))
        {
            return Result.Failure(
                $"'{nameof(seconds)}' должно быть от {MinTimeLimitSeconds} до {MaxTimeLimitSeconds} секунд.");
        }

        TimeLimitSeconds = seconds;
        Touch();

        return Result.Success();
    }

    public Result RemoveTimeLimit()
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        TimeLimitSeconds = null;
        Touch();

        return Result.Success();
    }

    public Result ChangeCoverImage(Guid fileAssetId)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        CoverImageId = fileAssetId;
        Touch();

        return Result.Success();
    }

    public Result RemoveCoverImage()
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        CoverImageId = null;
        Touch();

        return Result.Success();
    }

    public Result Publish()
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        if (_questions.Count == 0)
        {
            return Result.Failure("test.questions_required");
        }

        Status = TestStatus.Published;
        PublishedAt = DateTime.UtcNow;
        Touch();
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status != TestStatus.Published)
        {
            return Result.Failure("test.invalid_status_transition");
        }

        Status = TestStatus.Archived;
        Touch();
        return Result.Success();
    }

    private static Result Validate(
        string title,
        string description)
    {
        var titleResult = ValidateTitle(title);
        if (titleResult.IsFailure)
        {
            return Result.Failure(titleResult.Error);
        }

        var descriptionResult = ValidateDescription(description);
        if (descriptionResult.IsFailure)
        {
            return Result.Failure(descriptionResult.Error);
        }

        return Result.Success();
    }

    private static Result ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > MaxLengthTitle)
        {
            return Result.Failure("test.invalid_title");
        }

        return Result.Success();
    }

    private static Result ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > MaxLengthDescription)
        {
            return Result.Failure("test.invalid_description");
        }

        return Result.Success();
    }

    private void Reorder()
    {
        var ordered = _questions
            .OrderBy(x => x.Order)
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].SetOrder(i + 1);
        }
    }

    private Result EnsureDraft() =>
        Status == TestStatus.Draft
            ? Result.Success()
            : Result.Failure("test.not_editable");

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
