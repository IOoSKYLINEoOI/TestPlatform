using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Mappers;

public static class AttemptHistoryMapper
{
    public static AttemptHistoryItemResponse ToHistoryResponse(this Attempt attempt, string sourceTitle)
    {
        var result = attempt.AttemptResult;
        return attempt.Type switch
        {
            AttemptType.Test => new TestAttemptHistoryItemResponse(
                attempt.Id,
                attempt.AttemptNumber,
                attempt.SourceId,
                sourceTitle,
                attempt.Status.ToDto(),
                attempt.TotalQuestions,
                attempt.AnsweredQuestions,
                result?.CorrectAnswers,
                result is null ? null : (double)result.CorrectAnswers / attempt.TotalQuestions * 100,
                attempt.StartedAt,
                attempt.FinishedAt),
            AttemptType.Exam => new ExamAttemptHistoryItemResponse(
                attempt.Id,
                attempt.AttemptNumber,
                attempt.SourceId,
                sourceTitle,
                attempt.Status.ToDto(),
                attempt.TotalQuestions,
                attempt.AnsweredQuestions,
                result?.CorrectAnswers,
                result?.EarnedPoints,
                attempt.TotalMaxScore,
                result is null ? null : (double)(result.EarnedPoints / attempt.TotalMaxScore * 100),
                result?.Passed,
                attempt.StartedAt,
                attempt.FinishedAt),
            _ => throw new InvalidOperationException($"Unsupported attempt type: {attempt.Type}."),
        };
    }
}
