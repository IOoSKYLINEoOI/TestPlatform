using System.ComponentModel.DataAnnotations;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record UserAnswer(
    [Required] Guid QuestionId,
    [Required] List<Guid> AnswerId);
