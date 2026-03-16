using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Tests.DTOs;

public record TestFullResponse(
    Guid Id,
    string Name,
    int? TimeLimitSeconds,
    string Description,
    Guid? AuthorId,
    int TotalQuestions,
    string? CoverImageName,
    List<TagResponse> Tags,
    List<QuestionResponse> Questions);