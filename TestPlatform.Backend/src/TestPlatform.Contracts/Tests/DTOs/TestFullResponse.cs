using TestPlatform.Contracts.Tests.Enums;

namespace TestPlatform.Contracts.Tests.DTOs;

public record TestFullResponse(
    Guid Id,
    string Title,
    string Description,
    int? TimeLimitSeconds,
    Guid? CoverImageId,
    Guid? AuthorId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt,
    TestStatusDto Status,
    IReadOnlyCollection<TestQuestionResponse> Questions);

public record TestQuestionResponse(
    Guid QuestionId,
    int Order);
