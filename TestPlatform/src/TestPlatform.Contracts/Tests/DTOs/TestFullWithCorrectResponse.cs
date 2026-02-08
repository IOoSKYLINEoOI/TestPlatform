namespace TestPlatform.Contracts.Tests.DTOs;

public record TestFullWithCorrectResponse(
    int Id,
    string Name,
    int? TimeLimitSeconds,
    string Description,
    List<QuestionWithCorrectResponses> Questions);

public record QuestionWithCorrectResponses(
    int Id,
    string Text,
    int QuestionTypeId,
    List<AnswerWithCorrectResponse> Answers);

public record AnswerWithCorrectResponse(
    int Id,
    string Text,
    bool Correct);