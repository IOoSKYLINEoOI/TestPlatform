using CSharpFunctionalExtensions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Application.Attempts.CheckQuestionsService;

public class SingleChoiceChecker: IQuestionChecker
{
    public Result<bool> Check(QuestionResponse question, UserAnswer finishRequest)
    {
        if (finishRequest.AnswerId.Count != 1)
            return Result.Success(false);

        var correctAnswer = question.AnswerOptions
            .FirstOrDefault(a => a.IsCorrect == true);

        if (correctAnswer is null)
            return Result.Failure<bool>("Нет правильного ответа для вопроса");

        return Result.Success(finishRequest.AnswerId[0] == correctAnswer.Id);
    }
}