namespace TestPlatform.Contracts.Exams.DTOs;

public record UpdateExamScheduleRequest(DateTime? AvailableFrom, DateTime? AvailableTo);