using TestPlatform.Application.Attempts.Services.SourceService;
using TestPlatform.Application.Questions.Extensions;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Mappers;

public static class AttemptStartMapper
{
    public static AttemptStartSourceResponse ToStartResponse(
        AttemptSource source,
        AttemptType type,
        Guid attemptId)
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
                    q.Question.ToAttemptResponse(CreateShuffleSeed(attemptId, q.Question.Id))))
                .ToList());
    }

    private static int CreateShuffleSeed(Guid attemptId, Guid questionId)
    {
        var attemptBytes = attemptId.ToByteArray();
        var questionBytes = questionId.ToByteArray();
        var seed = 17;
        for (var index = 0; index < attemptBytes.Length; index++)
        {
            seed = unchecked(seed * 31 + (attemptBytes[index] ^ questionBytes[index]));
        }

        return seed;
    }
}
