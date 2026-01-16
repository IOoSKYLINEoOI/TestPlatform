namespace TestPlatform.Contracts.TestDTOs;

public record TestResponse(
    int Id,
    string Name,
    int? TimeLimitSeconds,
    string Description,
    int TotalQuestions,
    List<int> CategoryIds);