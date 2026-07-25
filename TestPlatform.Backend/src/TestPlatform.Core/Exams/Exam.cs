using CSharpFunctionalExtensions;
using TestPlatform.Core.Exams.Enums;

namespace TestPlatform.Core.Exams;

public class Exam
{
    private const int MaxLengthTitle = 200;
    private const int MaxLengthDescription = 250;
    private const int MinTimeLimitSeconds = 100;
    private const int MaxTimeLimitSeconds = 14_100;
    private const int MinQuestionsToPublish = 3;
    private const int MaxSelectedQuestions = 100;
    private const int MaxAttemptsLimit = 20;
    private readonly List<ExamSection> _sections = new();

    private Exam()
    {
    }

    private Exam(Guid id, string title, string description, Guid authorId)
    {
        Id = id;
        Title = title;
        Description = description;
        AuthorId = authorId;
        AttemptsLimit = 1;
        ReviewPolicy = ExamReviewPolicy.AfterExamClosed;
        Status = ExamStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public int? TimeLimitSeconds { get; private set; }
    public Guid? CoverImageId { get; private set; }
    public Guid AuthorId { get; private set; }
    public ExamStatus Status { get; private set; }
    public int AttemptsLimit { get; private set; }
    public ExamReviewPolicy ReviewPolicy { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? PublishedAt { get; private set; }
    public ExamSchedule? Schedule { get; private set; }
    public ExamPassingRule? PassingRule { get; private set; }
    public IReadOnlyCollection<ExamSection> Sections => _sections.AsReadOnly();
    public int TotalQuestions => _sections.Sum(section => section.QuestionsToSelect);
    public int TotalMaxScore => _sections.Sum(section => section.MaxScore);

    public static Result<Exam> Create(string title, string description, Guid authorId)
    {
        var validation = Validate(title, description);
        if (validation.IsFailure)
        {
            return Result.Failure<Exam>(validation.Error);
        }

        if (authorId == Guid.Empty)
        {
            return Result.Failure<Exam>("exam.author_required");
        }

        return Result.Success(new Exam(Guid.NewGuid(), title.Trim(), description.Trim(), authorId));
    }

    public Result<Guid> AddSection(string name, int questionsToSelect, int scorePerQuestion)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return Result.Failure<Guid>(editable.Error);
        }

        var sectionResult = ExamSection.Create(name, questionsToSelect, scorePerQuestion);
        if (sectionResult.IsFailure)
        {
            return Result.Failure<Guid>(sectionResult.Error);
        }

        if (TotalQuestions + questionsToSelect > MaxSelectedQuestions)
        {
            return Result.Failure<Guid>("exam.questions.limit_reached");
        }

        _sections.Add(sectionResult.Value);
        return Result.Success(sectionResult.Value.Id);
    }

    public Result RemoveSection(Guid sectionId)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        var section = _sections.FirstOrDefault(item => item.Id == sectionId);
        if (section is null)
        {
            return Result.Failure("exam.section.not_found");
        }

        _sections.Remove(section);
        return Result.Success();
    }

    public Result UpdateSection(Guid sectionId, string name, int questionsToSelect, int scorePerQuestion)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        var section = _sections.FirstOrDefault(item => item.Id == sectionId);
        if (section is null)
        {
            return Result.Failure("exam.section.not_found");
        }

        if (TotalQuestions - section.QuestionsToSelect + questionsToSelect > MaxSelectedQuestions)
        {
            return Result.Failure("exam.questions.limit_reached");
        }

        return section.Update(name, questionsToSelect, scorePerQuestion);
    }

    public Result AddQuestionToSection(Guid sectionId, Guid questionId)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        if (_sections.Any(section => section.QuestionIds.Contains(questionId)))
        {
            return Result.Failure("exam.question.already_in_pool");
        }

        var section = _sections.FirstOrDefault(item => item.Id == sectionId);
        return section is null
            ? Result.Failure("exam.section.not_found")
            : section.AddQuestion(questionId);
    }

    public Result RemoveQuestionFromSection(Guid sectionId, Guid questionId)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        var section = _sections.FirstOrDefault(item => item.Id == sectionId);
        return section is null
            ? Result.Failure("exam.section.not_found")
            : section.RemoveQuestion(questionId);
    }

    public Result ChangeAttemptsLimit(int attemptsLimit)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        if (attemptsLimit is < 1 or > MaxAttemptsLimit)
        {
            return Result.Failure("exam.invalid_attempts_limit");
        }

        AttemptsLimit = attemptsLimit;
        return Result.Success();
    }

    public Result ChangeReviewPolicy(ExamReviewPolicy reviewPolicy)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        if (!Enum.IsDefined(reviewPolicy))
        {
            return Result.Failure("exam.invalid_review_policy");
        }

        ReviewPolicy = reviewPolicy;
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
        return Result.Success();
    }

    public Result ChangeTimeLimit(int? seconds)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        if (seconds.HasValue && seconds is < MinTimeLimitSeconds or > MaxTimeLimitSeconds)
        {
            return Result.Failure("exam.invalid_time_limit");
        }

        TimeLimitSeconds = seconds;
        return Result.Success();
    }

    public Result RemoveTimeLimit() => ChangeTimeLimit(null);

    public Result ChangeSchedule(ExamSchedule schedule)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        Schedule = schedule;
        return Result.Success();
    }

    public Result RemoveSchedule()
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        Schedule = null;
        return Result.Success();
    }

    public Result ChangePassingRule(ExamPassingRule rule)
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        PassingRule = rule;
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
        return Result.Success();
    }

    public Result Publish()
    {
        var editable = EnsureDraft();
        if (editable.IsFailure)
        {
            return editable;
        }

        if (TotalQuestions < MinQuestionsToPublish)
        {
            return Result.Failure("exam.insufficient_questions");
        }

        if (PassingRule is null)
        {
            return Result.Failure("exam.passing_rule_required");
        }

        foreach (var section in _sections)
        {
            var validation = section.ValidateForPublication();
            if (validation.IsFailure)
            {
                return validation;
            }
        }
        if (ReviewPolicy == ExamReviewPolicy.AfterExamClosed && Schedule?.AvailableTo is null)
        {
            return Result.Failure("exam.review_requires_end_date");
        }

        if (PassingRule.MinScore > TotalMaxScore)
        {
            return Result.Failure("exam.passing_score_exceeds_maximum");
        }

        Status = ExamStatus.Published;
        PublishedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status != ExamStatus.Published)
        {
            return Result.Failure("exam.invalid_status_transition");
        }

        Status = ExamStatus.Archived;
        return Result.Success();
    }

    private Result EnsureDraft() => Status == ExamStatus.Draft
        ? Result.Success()
        : Result.Failure("exam.not_editable");

    private static Result Validate(string title, string description)
    {
        var titleResult = ValidateTitle(title);
        return titleResult.IsFailure ? titleResult : ValidateDescription(description);
    }

    private static Result ValidateTitle(string title) =>
        string.IsNullOrWhiteSpace(title) || title.Trim().Length > MaxLengthTitle
            ? Result.Failure("exam.invalid_title")
            : Result.Success();

    private static Result ValidateDescription(string description) =>
        string.IsNullOrWhiteSpace(description) || description.Trim().Length > MaxLengthDescription
            ? Result.Failure("exam.invalid_description")
            : Result.Success();
}
