using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Attempts.Interfaces;

public interface IQuestionCheckerFactory
{
    IQuestionChecker GetChecker(QuestionType questionType);
}