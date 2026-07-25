using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Questions.AnswerDefinition;

public class AnswerOption
{
    private const int MaxLengthText = 200;

    private AnswerOption(Guid id, string text, bool isCorrect, Guid? imageId)
    {
        Id = id;
        Text = text;
        IsCorrect = isCorrect;
        ImageId = imageId;
    }

    public Guid Id { get; }

    public string Text { get; }

    public bool IsCorrect { get; }

    public Guid? ImageId { get; }

    public static Result<AnswerOption> Create(string text, bool isCorrect, Guid? imageId) =>
        Create(Guid.NewGuid(), text, isCorrect, imageId);

    public static Result<AnswerOption> Create(Guid id, string text, bool isCorrect, Guid? imageId)
    {
        var validator = Validate(text);
        if (validator.IsFailure)
        {
            return Result.Failure<AnswerOption>(validator.Error);
        }

        if (id == Guid.Empty)
        {
            return Result.Failure<AnswerOption>("question.answer.option_id_required");
        }

        return Result.Success(new AnswerOption(id, text.Trim(), isCorrect, imageId));
    }

    private static Result Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLengthText)
        {
            return Result.Failure<AnswerOption>("question.answer.invalid_option_text");
        }

        return Result.Success();
    }
}
