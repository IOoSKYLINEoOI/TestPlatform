using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Questions.AnswerDefinition;

public class MatchingItem
{
    private const int MaxLengthText = 200;

    private MatchingItem(Guid id, string text, Guid? imageId)
    {
        Id = id;
        Text = text;
        ImageId = imageId;
    }

    public Guid Id { get; }

    public string Text { get; }

    public Guid? ImageId { get; }

    public static Result<MatchingItem> Create(string text, Guid? imageId)
    {
        return Create(Guid.NewGuid(), text, imageId);
    }

    public static Result<MatchingItem> Create(Guid id, string text, Guid? imageId)
    {
        var validator = Validate(text);
        if (validator.IsFailure)
            return Result.Failure<MatchingItem>(validator.Error);

        if (id == Guid.Empty)
            return Result.Failure<MatchingItem>("Идентификатор элемента сопоставления обязателен.");

        return Result.Success(new MatchingItem(id, text, imageId));
    }

    private static Result Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLengthText)
            return Result.Failure<MatchingItem>($"'{nameof(text)}' не может быть пустым или длиннее {MaxLengthText} символов.");

        return Result.Success();
    }
}
