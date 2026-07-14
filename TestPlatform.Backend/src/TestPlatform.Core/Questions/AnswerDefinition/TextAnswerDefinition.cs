using CSharpFunctionalExtensions;
using TestPlatform.Core.Questions.AnswerDefinition.Abstractions;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Core.Questions.AnswerDefinition;

public class TextAnswerDefinition : TypedQuestionAnswerDefinition<string>
{
    private TextAnswerDefinition(string correctAnswer)
    {
        CorrectAnswer = correctAnswer;
    }

    public override QuestionType Type => QuestionType.Text;

    public string CorrectAnswer { get; }

    public static Result<TextAnswerDefinition> Create(string correctAnswer)
    {
        if (string.IsNullOrWhiteSpace(correctAnswer))
            return Result.Failure<TextAnswerDefinition>("Правильный ответ обязателен.");

        return Result.Success(new TextAnswerDefinition(correctAnswer.Trim()));
    }

    public override decimal GetScore(string answer)
    {
        string normalizedUserAnswer = answer.Trim();
        string normalizedCorrectAnswer = CorrectAnswer.Trim();

        return string.Equals(
            normalizedUserAnswer,
            normalizedCorrectAnswer,
            StringComparison.OrdinalIgnoreCase)
            ? 1m
            : 0m;
    }
}