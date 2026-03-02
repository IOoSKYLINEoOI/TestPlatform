using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Questions;

public class Question
{
    private const int MaxAnswers = 50;
    private const int MaxLengthText = 200;

    private readonly List<AnswerOption> _answersOptions = new();
    private readonly List<Guid> _tagIds = new();

    private Question(Guid id, string text, QuestionType questionType, int points, string? imageUrl)
    {
        Id = id;
        Text = text;
        QuestionType = questionType;
        Points = points;
        ImageUrl = imageUrl;
    }

    public Guid Id { get; }

    public string Text { get; private set; }

    public QuestionType QuestionType { get; private set; }

    public int Points { get; private set; }

    public string? ImageUrl { get; private set; }

    public IReadOnlyCollection<AnswerOption> AnswersOptions => _answersOptions.AsReadOnly();

    public IReadOnlyCollection<Guid> TagIds => _tagIds.AsReadOnly();

    // -------------------- CREATE --------------------
    public static Result<Question> Create(string text, QuestionType questionType, int points, string? imageUrl)
    {
        var validation = Validate(text);
        if (validation.IsFailure)
            return Result.Failure<Question>(validation.Error);

        return Result.Success(new Question(Guid.NewGuid(), text, questionType, points, imageUrl));
    }

    public static Result<Question> CreateWithId(Guid id, string text, QuestionType questionType, int points, string? imageUrl)
    {
        var validation = Validate(text);
        if (validation.IsFailure)
            return Result.Failure<Question>(validation.Error);

        return Result.Success(new Question(id, text, questionType, points, imageUrl));
    }

    // -------------------- UPDATE --------------------
    public Result Update(string text, QuestionType questionType, int points, string? imageUrl)
    {
        var validation = Validate(text);
        if (validation.IsFailure)
            return validation;

        Text = text;
        QuestionType = questionType;
        Points = points;
        ImageUrl = imageUrl;

        return Result.Success();
    }

    // -------------------- ANSWERS --------------------
    public Result AddAnswerOption(AnswerOption answerOption)
    {
        if (_answersOptions.Count >= MaxAnswers)
            return Result.Failure($"Нельзя добавить больше {MaxAnswers} вариантов ответа.");

        if (QuestionType == QuestionType.SingleChoice && answerOption.IsCorrect &&
            _answersOptions.Any(a => a.IsCorrect))
            return Result.Failure("Для вопроса с одним вариантом может быть только один правильный ответ.");

        _answersOptions.Add(answerOption);
        return Result.Success();
    }

    public Result ReplaceAnswers(IEnumerable<AnswerOption> answers)
    {
        var list = answers.ToList();

        if (list.Count == 0)
            return Result.Failure("Вопрос должен содержать минимум один вариант ответа.");

        if (list.Count > MaxAnswers)
            return Result.Failure($"Нельзя добавить больше {MaxAnswers} ответов.");

        if (QuestionType == QuestionType.SingleChoice &&
            list.Count(a => a.IsCorrect) != 1)
            return Result.Failure("SingleChoice должен иметь ровно один правильный ответ.");

        if (QuestionType == QuestionType.MultipleChoice &&
            list.Count(a => a.IsCorrect) < 1)
            return Result.Failure("MultipleChoice должен иметь минимум один правильный ответ.");

        _answersOptions.Clear();
        _answersOptions.AddRange(list);

        return Result.Success();
    }

    // -------------------- TAGS --------------------
    public Result AddTag(Guid tagId)
    {
        if (_tagIds.Contains(tagId))
            return Result.Failure("Такой тег уже добавлен.");

        _tagIds.Add(tagId);
        return Result.Success();
    }

    public Result AddTags(IEnumerable<Guid> tagIds)
    {
        foreach (var tagId in tagIds)
        {
            var result = AddTag(tagId);
            if (result.IsFailure)
                return result;
        }
        return Result.Success();
    }

    public Result RemoveTag(Guid tagId)
    {
        if (!_tagIds.Contains(tagId))
            return Result.Failure("Такой тег не найден.");

        _tagIds.Remove(tagId);
        return Result.Success();
    }

    public void ReplaceTags(IEnumerable<Guid> tagIds)
    {
        _tagIds.Clear();
        _tagIds.AddRange(tagIds.Distinct());
    }

    // -------------------- VALIDATION --------------------
    private static Result Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLengthText)
            return Result.Failure($"'{nameof(text)}' не может быть пустым или длиннее {MaxLengthText} символов.");

        return Result.Success();
    }
}
