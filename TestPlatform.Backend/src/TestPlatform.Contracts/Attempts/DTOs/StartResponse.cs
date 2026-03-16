namespace TestPlatform.Contracts.Attempts.DTOs;

public record StartResponse(Guid AttemptId, AttemptSourceResponse SourceResponse);