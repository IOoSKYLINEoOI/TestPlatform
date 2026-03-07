namespace TestPlatform.Contracts.Tests.DTOs;

public record TestResponse(
    Guid Id,
    string Name,
    int? TimeLimitSeconds,
    string Description,
    Guid? AuthorId,
    int TotalQuestions,
    List<Guid> TagIds);