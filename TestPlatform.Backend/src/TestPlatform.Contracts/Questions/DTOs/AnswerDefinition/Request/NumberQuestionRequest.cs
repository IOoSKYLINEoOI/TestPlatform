namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record NumberQuestionRequest(
    string Text,
    Guid? ImageId,
    List<Guid> TagIds,
    decimal CorrectAnswer)
    : QuestionRequest(Text, ImageId, TagIds);