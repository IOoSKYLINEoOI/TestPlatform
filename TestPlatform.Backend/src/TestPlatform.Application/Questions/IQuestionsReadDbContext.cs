using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions;

public interface IQuestionsReadDbContext
{
    IQueryable<Question> ReadQuestions { get; }
}
