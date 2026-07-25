namespace TestPlatform.Core.Exams;

public class ExamSectionQuestion
{
    private ExamSectionQuestion()
    {
    }

    public ExamSectionQuestion(Guid questionId)
    {
        QuestionId = questionId;
    }

    public Guid QuestionId { get; }
}
