namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record TextQuestionRequest(
    string Text,
    string? Explanation,
    Guid? ImageId,
    List<Guid> TagIds,
    string CorrectAnswer)
    : QuestionRequest(Text, Explanation, ImageId, TagIds);
