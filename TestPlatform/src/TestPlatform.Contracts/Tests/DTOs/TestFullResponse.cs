namespace TestPlatform.Contracts.Tests.DTOs;

public record TestFullResponse(
    int Id,
    string Name,
    int? TimeLimitSeconds,
    string Description,
    List<QuestionFullResponse> Questions);

public record QuestionFullResponse(
    int Id,
    string Text,
    int QuestionTypeId,
    List<AnswerWithoutCorrectResponse> Answers);

public record AnswerWithoutCorrectResponse(
    int Id,
    string Text);