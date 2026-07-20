namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record TextQuestionRequest(
    string Text,
    Guid? ImageId,
    List<Guid> TagIds,
    string CorrectAnswer)
    : QuestionRequest(Text, ImageId, TagIds);