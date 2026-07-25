using TestPlatform.Application.Attempts.Services.SourceService;
using TestPlatform.Application.Questions.Extensions;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Mappers;

public static class AttemptStartMapper
{
    public static AttemptStartSourceResponse ToStartResponse(
        AttemptSource source,
        AttemptType type)
    {
        return new AttemptStartSourceResponse(
            source.TimeLimitSeconds,
            source.TotalQuestions,
            type.ToDto(),
            source.Questions
                .OrderBy(q => q.Order)
                .Select(q => new QuestionAssignmentResponse(
                    q.Order,
                    type == AttemptType.Exam ? q.Score : null,
                    q.Question.ToAttemptResponse()))
                .ToList());
    }
}
