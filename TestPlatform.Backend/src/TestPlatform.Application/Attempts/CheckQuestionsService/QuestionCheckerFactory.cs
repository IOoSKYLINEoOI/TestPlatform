using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Attempts.CheckQuestionsService;

public class QuestionCheckerFactory : IQuestionCheckerFactory
{
    public IQuestionChecker GetChecker(QuestionType questionType)
        => questionType switch
        {
            QuestionType.SingleChoice => new SingleChoiceChecker(),
            QuestionType.MultipleChoice => new MultipleChoiceChecker(),
            QuestionType.Text => new TextAnswerChecker(),
            QuestionType.Number => new NumberAnswerChecker(),
            QuestionType.Matching => new MatchingAnswerChecker(),
            _ => throw new NotSupportedException($"Тип вопроса {questionType} не поддерживается")
        };
}