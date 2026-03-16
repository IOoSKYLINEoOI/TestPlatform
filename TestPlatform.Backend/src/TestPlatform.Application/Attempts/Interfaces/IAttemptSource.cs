using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Application.Attempts.Interfaces;

public interface IAttemptSource
{
    Guid Id { get; }

    int? TimeLimitSeconds { get; }

    IReadOnlyCollection<QuestionResponse> Questions { get; }
}