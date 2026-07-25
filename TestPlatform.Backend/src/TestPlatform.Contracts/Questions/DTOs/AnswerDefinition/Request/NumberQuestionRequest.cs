namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record NumberQuestionRequest(
    string Text,
    string? Explanation,
    Guid? ImageId,
    List<Guid> TagIds,
    decimal CorrectAnswer)
    : QuestionRequest(Text, Explanation, ImageId, TagIds);
