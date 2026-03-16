using CSharpFunctionalExtensions;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Application.Attempts.Interfaces;

public interface IQuestionChecker
{
    Result<bool> Check(QuestionResponse question, UserAnswer finishRequest);
}