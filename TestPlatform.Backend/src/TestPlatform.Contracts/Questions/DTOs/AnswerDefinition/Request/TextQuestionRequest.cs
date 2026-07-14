namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record TextQuestionRequest(
    string Text,
    string? ImageName,
    List<Guid> TagIds,
    string CorrectAnswer)
    : QuestionRequest(Text, ImageName, TagIds);