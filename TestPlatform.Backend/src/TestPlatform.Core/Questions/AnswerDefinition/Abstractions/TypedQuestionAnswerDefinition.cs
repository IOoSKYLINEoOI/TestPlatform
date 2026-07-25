namespace TestPlatform.Core.Questions.AnswerDefinition.Abstractions;

public abstract class TypedQuestionAnswerDefinition<TAnswer> : QuestionAnswerDefinition
{
    public abstract decimal GetScore(TAnswer answer);

    public override decimal Evaluate(object answer)
    {
        if (answer is not TAnswer typed)
        {
            return 0m;
        }

        return GetScore(typed);
    }
}
