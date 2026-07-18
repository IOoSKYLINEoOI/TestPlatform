namespace TestPlatform.Contracts.Questions.DTOs;

public abstract record QuestionRequest(
    string Text,
    Guid? ImageId,
    List<Guid> TagIds);