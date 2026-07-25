namespace TestPlatform.Contracts.Exams.DTOs;

public record UpdateExamPassingRuleRequest(int? MinScore, double? MinPercent);
