namespace TestPlatform.Contracts.TestDTOs;

public record StartTestResponse(
    TestFullResponse TestFullResponse,
    int TestAttemptId);