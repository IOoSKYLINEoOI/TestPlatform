using CSharpFunctionalExtensions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Application.Attempts.CheckQuestionsService;

public class MultipleChoiceChecker : IQuestionChecker
{
    public Result<bool> Check(QuestionResponse question, UserAnswer finishRequest)
    {
        var correctIds = question.AnswerOptions
            .Where(a => a.IsCorrect == true)
            .Select(a => a.Id)
            .ToHashSet();

        var userIds = finishRequest.AnswerId.ToHashSet();

        return Result.Success(correctIds.SetEquals(userIds));
    }
}