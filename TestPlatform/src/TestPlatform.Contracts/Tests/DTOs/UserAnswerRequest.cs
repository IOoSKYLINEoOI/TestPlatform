using System.ComponentModel.DataAnnotations;

namespace TestPlatform.Contracts.TestDTOs;

public record UserAnswerRequest(
    [Required] int QuestionId,
    [Required] List<int> AnswerId);