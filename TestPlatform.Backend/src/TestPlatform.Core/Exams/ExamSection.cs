using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Exams;

public class ExamSection
{
    private const int MaxNameLength = 200;
    private const int MaxPoolSize = 200;
    private const int MaxQuestionsToSelect = 100;
    private const int MaxScorePerQuestion = 1_000;
    private readonly List<ExamSectionQuestion> _questions = new();

    private ExamSection()
    {
    }

    private ExamSection(Guid id, string name, int questionsToSelect, int scorePerQuestion)
    {
        Id = id;
        Name = name;
        QuestionsToSelect = questionsToSelect;
        ScorePerQuestion = scorePerQuestion;
    }

    public Guid Id { get; }

    public string Name { get; private set; } = null!;

    public int QuestionsToSelect { get; private set; }

    public int ScorePerQuestion { get; private set; }

    public IReadOnlyCollection<ExamSectionQuestion> Questions => _questions.AsReadOnly();

    public IReadOnlyCollection<Guid> QuestionIds => _questions.Select(x => x.QuestionId).ToList().AsReadOnly();

    public int MaxScore => QuestionsToSelect * ScorePerQuestion;

    public static Result<ExamSection> Create(string name, int questionsToSelect, int scorePerQuestion)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > MaxNameLength)
        {
            return Result.Failure<ExamSection>("exam.section.invalid_name");
        }

        if (questionsToSelect is <= 0 or > MaxQuestionsToSelect)
        {
            return Result.Failure<ExamSection>("exam.section.invalid_selection_count");
        }

        if (scorePerQuestion is <= 0 or > MaxScorePerQuestion)
        {
            return Result.Failure<ExamSection>("exam.section.invalid_score");
        }

        return Result.Success(new ExamSection(Guid.NewGuid(), name.Trim(), questionsToSelect, scorePerQuestion));
    }

    public Result Update(string name, int questionsToSelect, int scorePerQuestion)
    {
        var validation = Create(name, questionsToSelect, scorePerQuestion);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Error);
        }

        Name = name.Trim();
        QuestionsToSelect = questionsToSelect;
        ScorePerQuestion = scorePerQuestion;
        return Result.Success();
    }

    public Result AddQuestion(Guid questionId)
    {
        if (questionId == Guid.Empty)
        {
            return Result.Failure("exam.section.question_required");
        }

        if (_questions.Any(x => x.QuestionId == questionId))
        {
            return Result.Failure("exam.section.question_already_added");
        }

        if (_questions.Count >= MaxPoolSize)
        {
            return Result.Failure("exam.section.pool_limit_reached");
        }

        _questions.Add(new ExamSectionQuestion(questionId));
        return Result.Success();
    }

    public Result RemoveQuestion(Guid questionId)
    {
        var question = _questions.FirstOrDefault(x => x.QuestionId == questionId);
        if (question is null)
        {
            return Result.Failure("exam.section.question_not_found");
        }

        _questions.Remove(question);
        return Result.Success();
    }

    public Result ValidateForPublication()
    {
        if (_questions.Count < QuestionsToSelect)
        {
            return Result.Failure("exam.section.insufficient_pool");
        }

        return Result.Success();
    }
}
