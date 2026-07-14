using CSharpFunctionalExtensions;
using TestPlatform.Core.Questions.AnswerDefinition.Abstractions;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Core.Questions.AnswerDefinition;

public class NumberAnswerDefinition : TypedQuestionAnswerDefinition<decimal>
{
    private NumberAnswerDefinition(decimal correctAnswer)
    {
        CorrectAnswer = correctAnswer;
    }

    public override QuestionType Type => QuestionType.Number;

    public decimal CorrectAnswer { get; }

    public static Result<NumberAnswerDefinition> Create(decimal correctAnswer)
    {
        return Result.Success(new NumberAnswerDefinition(correctAnswer));
    }

    public override decimal GetScore(decimal answer)
    {
        decimal roundedUser = Math.Round(answer, 3);
        decimal roundedCorrect = Math.Round(CorrectAnswer, 3);

        return roundedUser == roundedCorrect
            ? 1m
            : 0m;
    }
}