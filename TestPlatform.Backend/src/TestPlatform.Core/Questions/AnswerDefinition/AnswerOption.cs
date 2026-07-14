using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Questions.AnswerDefinition;

public class AnswerOption
{
    private const int MaxLengthText = 200;

    private AnswerOption(Guid id, string text, bool isCorrect, string? imageName)
    {
        Id = id;
        Text = text;
        IsCorrect = isCorrect;
        ImageName = imageName;
    }

    public Guid Id { get; }

    public string Text { get; }

    public bool IsCorrect { get; }

    public string? ImageName { get; }

    public static Result<AnswerOption> Create(string text, bool isCorrect, string? imageName)
    {
        var validator = Validate(text);
        if(validator.IsFailure)
            return Result.Failure<AnswerOption>(validator.Error);

        return Result.Success(new AnswerOption(Guid.NewGuid(), text, isCorrect, imageName));
    }

    private static Result Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLengthText)
            return Result.Failure<AnswerOption>($"'{nameof(text)}' не может быть пустым или длиннее {MaxLengthText} символов.");

        return Result.Success();
    }
}