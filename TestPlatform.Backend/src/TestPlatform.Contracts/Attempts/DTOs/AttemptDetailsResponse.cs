using TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Response;
using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record AttemptDetailsResponse(
    Guid Id,
    AttemptStatusDto Status,
    int CorrectAnswers,
    int TotalQuestion,
    decimal EarnedPoints,
    decimal TotalMaxScore,
    IReadOnlyCollection<AttemptQuestionDetailsResponse> Questions);

public record AttemptQuestionDetailsResponse(
    int Order,
    decimal Score,
    QuestionResultResponse Question,
    AttemptAnswerResponse? UserAnswer);