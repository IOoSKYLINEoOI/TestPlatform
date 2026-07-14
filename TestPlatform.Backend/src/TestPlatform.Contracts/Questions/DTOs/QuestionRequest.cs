namespace TestPlatform.Contracts.Questions.DTOs;

public abstract record QuestionRequest(
    string Text,
    string? ImageName,
    List<Guid> TagIds);
