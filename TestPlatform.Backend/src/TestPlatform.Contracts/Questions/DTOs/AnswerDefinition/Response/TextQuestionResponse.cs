using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public record TextQuestionResponse(
    Guid Id,
    string Text,
    string? ImageName,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags)
    : QuestionResponse(Id, Text, ImageName, Tags);
