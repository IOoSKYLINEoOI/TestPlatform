using CSharpFunctionalExtensions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Application.Attempts.CheckQuestionsService;

public class NumberAnswerChecker : IQuestionChecker
{
    private IQuestionChecker _questionCheckerImplementation;
    public Result<bool> Check(QuestionResponse question, UserAnswer finishRequest) => _questionCheckerImplementation.Check(question, finishRequest);
}