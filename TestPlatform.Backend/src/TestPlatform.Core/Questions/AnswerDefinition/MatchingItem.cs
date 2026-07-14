using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Questions.AnswerDefinition;

public class MatchingItem
{
    private const int MaxLengthText = 200;

    private MatchingItem(Guid id, string text, string? imageName)
    {
        Id = id;
        Text = text;
        ImageName = imageName;
    }

    public Guid Id { get; }

    public string Text { get; }

    public string? ImageName { get; }

    public static Result<MatchingItem> Create(string text, string? imageName)
    {
        var validator = Validate(text);
        if(validator.IsFailure)
            return Result.Failure<MatchingItem>(validator.Error);

        return Result.Success(new MatchingItem(Guid.NewGuid(), text, imageName));
    }

    private static Result Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLengthText)
            return Result.Failure<MatchingItem>($"'{nameof(text)}' не может быть пустым или длиннее {MaxLengthText} символов.");

        return Result.Success();
    }
}