using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public record NumberQuestionResultResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    decimal CorrectAnswer,
    IReadOnlyList<TagResponse> Tags)
    : QuestionResultResponse(Id, Text, ImageId, Tags);