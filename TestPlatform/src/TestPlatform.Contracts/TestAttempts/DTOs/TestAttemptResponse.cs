namespace TestPlatform.Contracts.TestAttemptDTOs;

public record TestAttemptResponse(
    int Id,
    int TotalQuestions,
    int CorrectAnswers,
    double Score,
    DateTime StartedAt,
    DateTime? FinishedAt,
    int TestId);