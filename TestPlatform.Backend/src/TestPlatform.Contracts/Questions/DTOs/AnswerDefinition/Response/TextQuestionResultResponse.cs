using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public record TextQuestionResultResponse(Guid Id,
    string Text,
    string? ImageName,
    QuestionTypeDto Type,
    string CorrectAnswer,
    IReadOnlyList<TagResponse> Tags)
    : QuestionResultResponse(Id, Text, ImageName, Tags);
