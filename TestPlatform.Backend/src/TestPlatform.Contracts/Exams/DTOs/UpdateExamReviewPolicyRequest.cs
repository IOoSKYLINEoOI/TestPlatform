using System.Text.Json.Serialization;

namespace TestPlatform.Contracts.Exams.DTOs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExamReviewPolicyDto
{
    Immediately = 1,
    AfterExamClosed = 2,
}

public record UpdateExamReviewPolicyRequest(ExamReviewPolicyDto ReviewPolicy);
