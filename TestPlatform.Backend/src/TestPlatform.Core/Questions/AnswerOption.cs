using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Questions;

public class AnswerOption
{
    private const int MaxLengthText = 200;

    private AnswerOption(Guid id, string text, bool isCorrect, string? imageUrl)
    {
        Id = id;
        Text = text;
        IsCorrect = isCorrect;
        ImageUrl = imageUrl;
    }

    public Guid Id { get; }

    public string Text { get; }

    public bool IsCorrect { get; }

    public string? ImageUrl { get; }

    public static Result<AnswerOption> Create(string text, bool isCorrect, string? imageUrl)
    {
        var validator = Validate(text);
        if(validator.IsFailure)
            return Result.Failure<AnswerOption>(validator.Error);

        return Result.Success(new AnswerOption(Guid.NewGuid(), text, isCorrect, imageUrl));
    }

    public static Result<AnswerOption> CreateWithId(Guid id, string text, bool isCorrect, string? imageUrl)
    {
        var validator = Validate(text);
        if(validator.IsFailure)
            return Result.Failure<AnswerOption>(validator.Error);

        return Result.Success(new AnswerOption(id, text, isCorrect, imageUrl));
    }

    private static Result Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLengthText)
            return Result.Failure<AnswerOption>($"'{nameof(text)}' не может быть пустым или длиннее {MaxLengthText} символов.");

        return Result.Success();
    }
}