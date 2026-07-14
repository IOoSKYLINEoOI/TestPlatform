namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record NumberQuestionRequest(
    string Text,
    string? ImageName,
    List<Guid> TagIds,
    decimal CorrectAnswer)
    : QuestionRequest(Text, ImageName, TagIds);