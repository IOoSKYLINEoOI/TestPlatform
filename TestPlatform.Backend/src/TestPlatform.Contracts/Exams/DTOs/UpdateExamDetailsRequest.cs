namespace TestPlatform.Contracts.Exams.DTOs;

public record UpdateExamDetailsRequest(
    string? Title,
    string? Description);
