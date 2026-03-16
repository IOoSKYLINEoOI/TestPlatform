using CSharpFunctionalExtensions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Application.Attempts.CheckQuestionsService;

public class TextAnswerChecker : IQuestionChecker
{
    public Result<bool> Check(QuestionResponse question, UserAnswer finishRequest)
    {
        if (finishRequest.AnswerId.Count != 1)
            return Result.Success(false);

        var answerText = question.AnswerOptions.First().Text;
        //return Result.Success(answerText?.Trim().Equals(userAnswer.AnswerText?.Trim(), StringComparison.OrdinalIgnoreCase) ?? false);
        return false;
    }
}