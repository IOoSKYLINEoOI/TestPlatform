using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts.Mappers;

public static class AttemptFinishMapper
{
    public static FinishAttemptResponse ToFinishResponse(this Attempt attempt)
    {
        if (attempt.AttemptResult is null)
            throw new InvalidOperationException("Attempt is not finished");

        if (attempt.StartedAt is null || attempt.FinishedAt is null)
            throw new InvalidOperationException("Attempt dates are not set");

        return new FinishAttemptResponse(
            attempt.Id,
            attempt.TotalQuestions,
            attempt.AttemptResult.CorrectAnswers,
            attempt.AttemptResult.EarnedPoints,
            attempt.TotalMaxScore,
            attempt.StartedAt.Value,
            attempt.FinishedAt.Value,
            attempt.Status.ToDto());
    }
}