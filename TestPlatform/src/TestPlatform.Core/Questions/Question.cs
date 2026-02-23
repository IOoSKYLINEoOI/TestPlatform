using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Questions;

public class Question
{
    private const int MaxAnswers = 50;
    private const int MaxLengthText = 200;
    private readonly List<AnswerOption> _answerOptions = new();

    private Question(Guid id, string text, QuestionType questionType, int points, string? imageUrl)
    {
        Id = id;
        Text = text;
        QuestionType = questionType;
        Points = points;
        ImageUrl = imageUrl;
    }

    public Guid Id { get; }

    public string Text { get; }

    public QuestionType QuestionType { get; }

    public int Points { get; }

    public string? ImageUrl { get; }

    public IReadOnlyCollection<AnswerOption> Answers => _answerOptions.AsReadOnly();

    private int TotalAnswers => _answerOptions.Count;

    public static Result<Question> Create(string text, QuestionType questionType, int points, string? imageUrl)
    {
        var validation = Validate(text);
        if (validation.IsFailure)
            return Result.Failure<Question>(validation.Error);

        return Result.Success(new Question(Guid.NewGuid(), text, questionType, points, imageUrl));
    }

    public Result AddAnswerOption(AnswerOption answerOption)
    {
        if (TotalAnswers >= MaxAnswers)
            return Result.Failure($"Нельзя добавить больше {MaxAnswers} ответов.");

        if (QuestionType == QuestionType.SingleChoice && answerOption.IsCorrect &&
            _answerOptions.Any(a => a.IsCorrect))
            return Result.Failure("Для вопроса с одним вариантом может быть только один правильный ответ.");

        _answerOptions.Add(answerOption);

        return Result.Success();
    }

    private static Result Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLengthText)
            return Result.Failure<Question>($"'{nameof(text)}' не может быть пустым или длиннее {MaxLengthText} символов.");

        return Result.Success();
    }
}