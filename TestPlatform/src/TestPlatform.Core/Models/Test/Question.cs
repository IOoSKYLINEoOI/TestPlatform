using CSharpFunctionalExtensions;
using TestPlatform.Core.Enums;

namespace TestPlatform.Core.Models.Test;

public class Question
{
    private const int MaxAnswers = 50;
    private const int MaxLengthText = 200;
    private readonly List<Answer> _answers = new();
    
    private Question(Guid id, string text, QuestionType questionType, string? imageUrl)
    {
        Id = id;
        Text = text;
        QuestionType = questionType;
        ImageUrl = imageUrl;
    }

    public Guid Id { get; }
    public string Text { get; }
    public string? ImageUrl { get; }
    public QuestionType QuestionType { get; }

    public IReadOnlyCollection<Answer> Answers => _answers.AsReadOnly();
    private int TotalAnswers => _answers.Count;

    public static Result<Question> Create(string text, QuestionType questionType, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLengthText)
            return Result.Failure<Question>($"'{nameof(text)}' не может быть null или длиннее {MaxLengthText} символов.");

        return Result.Success(new Question(Guid.NewGuid(), text, questionType, imageUrl));
    }

    public static Question FromPersistence(Guid id, string text, QuestionType questionType, string? imageUrl, IEnumerable<Answer> answers)
    {
        var question = new Question(id, text, questionType, imageUrl);

        question._answers.AddRange(answers);
        
        return question;
    }
    
    public Result AddAnswer(Answer answer)
    {
        if (TotalAnswers >= MaxAnswers)
            return Result.Failure($"Нельзя добавить больше {MaxAnswers} ответов.");

        _answers.Add(answer);
        return Result.Success();
    }
}