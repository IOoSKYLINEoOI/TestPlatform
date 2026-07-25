namespace TestPlatform.Core.Attempts;

public class AttemptQuestionSelection
{
    private AttemptQuestionSelection() { }

    public AttemptQuestionSelection(Guid questionId, int order, decimal score)
    {
        QuestionId = questionId;
        Order = order;
        Score = score;
    }

    public Guid QuestionId { get; }

    public int Order { get; }

    public decimal Score { get; }
}
