namespace TestPlatform.Contracts.Tests.DTOs;

public record TestFullRequest(
    string Name,
    int? TimeLimitSeconds,
    string Description,
    List<int> tagsIds,
    List<QuestionFullRequest> Questions);

public record QuestionFullRequest(
    string Text,
    int QuestionTypeId,
    List<AnswerRequest> Answers);

public record AnswerRequest(
    string Text,
    bool IsCorrect);