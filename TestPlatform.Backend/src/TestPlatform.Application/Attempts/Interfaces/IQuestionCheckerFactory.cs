using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Application.Attempts.Interfaces;

public interface IQuestionCheckerFactory
{
    IQuestionChecker GetChecker(QuestionType questionType);
}