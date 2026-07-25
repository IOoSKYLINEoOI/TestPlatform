namespace TestPlatform.Core.Tests;

public class TestQuestion
{
    private TestQuestion()
    {
    }

    public TestQuestion(Guid questionId, int order)
    {
        QuestionId = questionId;
        Order = order;
    }

    public Guid QuestionId { get; }
    public int Order { get; private set; }

    public void SetOrder(int order) => Order = order;
}
