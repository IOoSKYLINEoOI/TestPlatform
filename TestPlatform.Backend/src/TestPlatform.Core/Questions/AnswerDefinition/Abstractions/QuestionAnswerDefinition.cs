using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Core.Questions.AnswerDefinition.Abstractions;

public abstract class QuestionAnswerDefinition
{
    public abstract QuestionType Type { get; }

    public abstract decimal Evaluate(object answer);
}