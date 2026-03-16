using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Exams.DTOs;

public record ExamFullResponse(
    Guid Id,
    string Name,
    int? TimeLimitSeconds,
    string Description,
    Guid? AuthorId,
    int TotalQuestions,
    List<TagResponse> Tags,
    List<QuestionResponse> Questions);


