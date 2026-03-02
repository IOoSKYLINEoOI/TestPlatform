namespace TestPlatform.Contracts.Tests.DTOs;

public record TestRequest(
    string Name,
    int? TimeLimitSeconds,
    string Description,
    Guid? AuthorId,
    string? CoverImageUrl,
    List<Guid> Questions);
