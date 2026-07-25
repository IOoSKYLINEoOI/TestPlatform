using TestPlatform.Contracts.Tests.Enums;

namespace TestPlatform.Contracts.Tests.DTOs;

public record TestResponse(
    Guid Id,
    string Title,
    string Description,
    int? TimeLimitSeconds,
    Guid? AuthorId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    TestStatusDto Status,
    int TotalQuestions);
