using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public abstract record QuestionResultResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    IReadOnlyList<TagResponse> Tags);