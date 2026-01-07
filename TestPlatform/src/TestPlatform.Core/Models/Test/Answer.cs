using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Models.Test;

public class Answer
{
    private const int MaxLengthText = 200;
    
    private Answer(Guid id, string text, bool isCorrect, string? imageUrl)
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

    public static Result<Answer> Create(string text, bool isCorrect, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLengthText)
            return Result.Failure<Answer>($"'{nameof(text)}' не может быть пустым или длиннее {MaxLengthText} символов.");

        return Result.Success(new Answer(Guid.NewGuid(),text, isCorrect, imageUrl));
    }

    public static Answer FromPersistence(Guid id, string text, bool isCorrect, string? imageUrl)
    {
        return new Answer(id, text, isCorrect, imageUrl);
    }
}