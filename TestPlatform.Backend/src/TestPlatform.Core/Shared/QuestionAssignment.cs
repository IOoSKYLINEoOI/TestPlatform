using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Shared;

public class QuestionAssignment
{
    public Guid QuestionId { get; private set; }

    public int Order { get; private set; }

    public int Score { get; private set; }

    public QuestionAssignment(Guid questionId, int order, int score)
    {
        QuestionId = questionId;
        Order = order;
        Score = score;
    }

    public Result SetOrder(int order)
    {
        if (order <= 0)
            return Result.Failure("Order должен быть больше 0");

        Order = order;
        return Result.Success();
    }
}