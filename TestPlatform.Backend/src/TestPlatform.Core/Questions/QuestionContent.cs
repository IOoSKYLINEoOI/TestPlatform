using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Questions;

public sealed class QuestionContent
{
    public const int MaxTextLength = 500;
    public const int MaxExplanationLength = 2_000;

    private QuestionContent()
    {
    }

    private QuestionContent(string text, string? explanation)
    {
        Text = text;
        Explanation = explanation;
    }

    public string Text { get; private set; } = null!;

    public string? Explanation { get; private set; }

    public static Result<QuestionContent> Create(string text, string? explanation)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<QuestionContent>("question.text_required");
        }

        var normalizedText = text.Trim();
        if (normalizedText.Length > MaxTextLength)
        {
            return Result.Failure<QuestionContent>("question.text_too_long");
        }

        var normalizedExplanation = string.IsNullOrWhiteSpace(explanation) ? null : explanation.Trim();
        if (normalizedExplanation?.Length > MaxExplanationLength)
        {
            return Result.Failure<QuestionContent>("question.explanation_too_long");
        }

        return Result.Success(new QuestionContent(normalizedText, normalizedExplanation));
    }
}
