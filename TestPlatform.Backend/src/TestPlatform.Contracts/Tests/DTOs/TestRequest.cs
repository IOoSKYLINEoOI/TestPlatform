namespace TestPlatform.Contracts.Tests.DTOs;

public record TestRequest(
    string Name,
    int? TimeLimitSeconds,
    string Description,
    string? CoverImageUrl,
    List<Guid> QuestionsIds);
