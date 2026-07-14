using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts.Services.SourceService;

public record AttemptSource(
    IReadOnlyCollection<AttemptQuestion> Questions,
    int TotalQuestions,
    decimal TotalMaxScore,
    int? TimeLimitSeconds);