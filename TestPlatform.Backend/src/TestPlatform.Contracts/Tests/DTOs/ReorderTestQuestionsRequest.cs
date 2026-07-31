namespace TestPlatform.Contracts.Tests.DTOs;

public sealed record ReorderTestQuestionsRequest(IReadOnlyList<Guid> QuestionIds);
