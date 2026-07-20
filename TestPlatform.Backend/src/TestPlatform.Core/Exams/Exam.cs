using CSharpFunctionalExtensions;
using TestPlatform.Core.Exams.Enums;
using TestPlatform.Core.Shared;

namespace TestPlatform.Core.Exams;

public class Exam
{
    private const int MaxQuestions = 100;
    private const int MaxLengthTitle = 200;
    private const int MaxLengthDescription = 250;
    private const int MinTimeLimitSeconds = 100;
    private const int MaxTimeLimitSeconds = 14100;
    private const int MinQuestionsToPublish = 3;

    private readonly List<QuestionAssignment> _questions = new();

    private Exam() { }

    private Exam(
        Guid id,
        string title,
        string description,
        Guid authorId)
    {
        Id = id;
        Title = title;
        Description = description;
        AuthorId = authorId;
        Status = ExamStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public int? TimeLimitSeconds { get; private set; }

    public Guid? CoverImageId { get; private set; }

    public Guid AuthorId { get; private set; }

    public ExamStatus Status { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? PublishedAt { get; private set; }

    public ExamSchedule? Schedule { get; private set; }

    public ExamPassingRule? PassingRule { get; private set; }

    public IReadOnlyCollection<QuestionAssignment> Questions => _questions.AsReadOnly();


    public static Result<Exam> Create(
        string title,
        string description,
        Guid authorId)
    {
        var validation = Validate(title, description);
        if (validation.IsFailure)
            return Result.Failure<Exam>(validation.Error);

        return Result.Success(
            new Exam(
                Guid.NewGuid(),
                title,
                description,
                authorId));
    }

    public Result AddQuestion(Guid questionId, int score)
    {
        if (!IsDraft())
            return Result.Failure("Можно изменять только черновик");

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
        if (!IsDraft())
            return Result.Failure("Можно изменять только черновик");

        var question = _questions.FirstOrDefault(x => x.QuestionId == questionId);
        if (question is null)
            return Result.Failure("Вопрос не найден");

        _questions.Remove(question);

        Reorder();

        return Result.Success();
    }

    public Result ChangeTitle(string title)
    {
        if (!IsDraft())
            return Result.Failure("Редактирование доступно только для черновика");

        var validation = ValidateTitle(title);
        if (validation.IsFailure)
            return validation;

        Title = title;

        return Result.Success();
    }

    public Result ChangeDescription(string description)
    {
        if (!IsDraft())
            return Result.Failure("Редактирование доступно только для черновика");

        var validation = ValidateDescription(description);
        if (validation.IsFailure)
            return validation;

        Description = description;

        return Result.Success();
    }

    public Result ChangeTimeLimit(int? seconds)
    {
        if (!IsDraft())
            return Result.Failure("Редактирование доступно только для черновика");

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
        if (!IsDraft())
            return Result.Failure("Редактирование доступно только для черновика");

        TimeLimitSeconds = null;

        return Result.Success();
    }

    public Result ChangeSchedule(ExamSchedule schedule)
    {
        if (!IsDraft())
            return Result.Failure("Редактирование доступно только для черновика");

        Schedule = schedule;

        return Result.Success();
    }

    public Result RemoveSchedule()
    {
        if (!IsDraft())
            return Result.Failure("Редактирование доступно только для черновика");

        Schedule = null;

        return Result.Success();
    }

    public Result ChangePassingRule(ExamPassingRule rule)
    {
        if (!IsDraft())
            return Result.Failure("Редактирование доступно только для черновика");

        PassingRule = rule;

        return Result.Success();
    }

    public Result ChangeCoverImage(Guid fileAssetId)
    {
        if (!IsDraft())
            return Result.Failure("Редактирование доступно только для черновика");

        CoverImageId = fileAssetId;

        return Result.Success();
    }

    public Result RemoveCoverImage()
    {
        if (!IsDraft())
            return Result.Failure("Редактирование доступно только для черновика");

        CoverImageId = null;

        return Result.Success();
    }

    public Result Publish()
    {
        if (!IsDraft())
            return Result.Failure("Можно публиковать только черновик");

        if (_questions.Count < MinQuestionsToPublish)
            return Result.Failure("Слишком мало вопросов для экзамена");

        if (PassingRule == null)
            return Result.Failure("Правила оценивания обязательны для публикации");

        Status = ExamStatus.Published;
        PublishedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Archive()
    {
        if (IsDraft())
            return Result.Failure("Нельзя архивировать черновик");

        if (IsArchived())
            return Result.Failure("Экзамен уже архивирован");

        Status = ExamStatus.Archived;

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

    private bool IsDraft() => Status == ExamStatus.Draft;

    private bool IsPublished() => Status == ExamStatus.Published;

    private bool IsArchived() => Status == ExamStatus.Archived;
}