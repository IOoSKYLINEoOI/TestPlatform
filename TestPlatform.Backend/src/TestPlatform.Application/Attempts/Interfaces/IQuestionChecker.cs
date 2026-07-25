using CSharpFunctionalExtensions;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Questions.DTOs.Passing;

namespace TestPlatform.Application.Attempts.Interfaces;

public interface IQuestionChecker
{
    Result<bool> Check(AttemptQuestionResponse question, UserAnswer finishRequest);
}
