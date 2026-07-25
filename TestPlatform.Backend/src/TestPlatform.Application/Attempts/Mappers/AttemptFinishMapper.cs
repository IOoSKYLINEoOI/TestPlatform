using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts.Mappers;

public static class AttemptFinishMapper
{
    public static AttemptResultResponse ToResultResponse(this Attempt attempt)
    {
        if (attempt.AttemptResult is null)
        {
            throw new InvalidOperationException("Attempt is not finished");
        }

        if (attempt.StartedAt is null || attempt.FinishedAt is null)
        {
            throw new InvalidOperationException("Attempt dates are not set");
        }

        var percentage = attempt.TotalQuestions == 0
            ? 0
            : (double)attempt.AttemptResult.CorrectAnswers / attempt.TotalQuestions * 100;

        return attempt.Type switch
        {
            Core.Attempts.Enums.AttemptType.Test => new TestAttemptResultResponse(
                attempt.Id,
                attempt.TotalQuestions,
                attempt.AttemptResult.CorrectAnswers,
                percentage,
                attempt.StartedAt.Value,
                attempt.FinishedAt.Value,
                attempt.Status.ToDto()),
            Core.Attempts.Enums.AttemptType.Exam => new ExamAttemptResultResponse(
                attempt.Id,
                attempt.TotalQuestions,
                attempt.AttemptResult.CorrectAnswers,
                (double)(attempt.AttemptResult.EarnedPoints / attempt.TotalMaxScore * 100),
                attempt.AttemptResult.EarnedPoints,
                attempt.TotalMaxScore,
                attempt.AttemptResult.Passed ?? false,
                attempt.StartedAt.Value,
                attempt.FinishedAt.Value,
                attempt.Status.ToDto()),
            _ => throw new InvalidOperationException($"Unsupported attempt type: {attempt.Type}."),
        };
    }
}
