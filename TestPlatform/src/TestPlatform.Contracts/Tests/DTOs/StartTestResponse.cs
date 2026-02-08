namespace TestPlatform.Contracts.Tests.DTOs;

public record StartTestResponse(
    TestFullResponse TestFullResponse,
    int TestAttemptId);