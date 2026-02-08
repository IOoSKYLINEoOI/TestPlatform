using System.ComponentModel.DataAnnotations;

namespace TestPlatform.Contracts.Tests.DTOs;

public record UserAnswerRequest(
    [Required] int QuestionId,
    [Required] List<int> AnswerId);