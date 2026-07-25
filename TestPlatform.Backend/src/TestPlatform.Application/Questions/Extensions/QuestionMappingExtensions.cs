using TestPlatform.Application.Questions.Mappers;
using TestPlatform.Application.Tags.Extensions;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;
using TestPlatform.Contracts.Questions.DTOs.Editor;
using TestPlatform.Contracts.Questions.DTOs.Passing;
using TestPlatform.Contracts.Questions.DTOs.Preview;
using TestPlatform.Contracts.Questions.DTOs.Results;
using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Core.Questions;
using TestPlatform.Core.Questions.AnswerDefinition;

namespace TestPlatform.Application.Questions.Extensions;

public static class QuestionMappingExtensions
{
    public static AttemptQuestionResponse ToAttemptResponse(this Question question)
    {
        var tags = question.Tags.ToResponses();

        return question.AnswerDefinition switch
        {
            ChoiceAnswerDefinition definition => new ChoiceAttemptQuestionResponse(
                question.Id,
                question.Text,
                question.ImageId,
                definition.Type.ToDto(),
                definition.Mode.ToDto(),
                definition.EvaluationMode.ToDto(),
                tags,
                definition.Options.Select(option => new AnswerOptionResponse(
                    option.Id,
                    option.Text,
                    option.ImageId)).ToList()),
            TextAnswerDefinition => new TextAttemptQuestionResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Text,
                tags),
            NumberAnswerDefinition => new NumberAttemptQuestionResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Number,
                tags),
            MatchingAnswerDefinition definition => new MatchingAttemptQuestionResponse(
                question.Id,
                question.Text,
                question.ImageId,
                definition.Type.ToDto(),
                definition.Mode.ToDto(),
                tags,
                ToMatchingItems(definition.LeftItems),
                ToMatchingItems(definition.RightItems)),
            _ => throw Unsupported(question),
        };
    }

    public static AttemptQuestionResultResponse ToAttemptResultResponse(this Question question)
    {
        var tags = question.Tags.ToResponses();

        return question.AnswerDefinition switch
        {
            ChoiceAnswerDefinition definition => new ChoiceAttemptQuestionResultResponse(
                question.Id,
                question.Text,
                question.ImageId,
                definition.Type.ToDto(),
                definition.Mode.ToDto(),
                definition.EvaluationMode.ToDto(),
                tags,
                question.Explanation,
                ToResultOptions(definition)),
            TextAnswerDefinition definition => new TextAttemptQuestionResultResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Text,
                tags,
                question.Explanation,
                definition.CorrectAnswer),
            NumberAnswerDefinition definition => new NumberAttemptQuestionResultResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Number,
                tags,
                question.Explanation,
                definition.CorrectAnswer),
            MatchingAnswerDefinition definition => new MatchingAttemptQuestionResultResponse(
                question.Id,
                question.Text,
                question.ImageId,
                definition.Type.ToDto(),
                definition.Mode.ToDto(),
                tags,
                question.Explanation,
                ToMatchingItems(definition.LeftItems),
                ToMatchingItems(definition.RightItems),
                ToMatchingPairs(definition)),
            _ => throw Unsupported(question),
        };
    }

    public static QuestionEditorResponse ToEditorResponse(this Question question)
    {
        var tags = question.Tags.ToResponses();
        var status = question.Status.ToDto();

        return question.AnswerDefinition switch
        {
            ChoiceAnswerDefinition definition => new ChoiceQuestionEditorResponse(
                question.Id,
                question.Text,
                question.ImageId,
                definition.Type.ToDto(),
                definition.Mode.ToDto(),
                definition.EvaluationMode.ToDto(),
                tags,
                question.Explanation,
                status,
                question.CreatedByUserId,
                question.CreatedAt,
                question.UpdatedAt,
                ToResultOptions(definition)),
            TextAnswerDefinition definition => new TextQuestionEditorResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Text,
                tags,
                question.Explanation,
                status,
                question.CreatedByUserId,
                question.CreatedAt,
                question.UpdatedAt,
                definition.CorrectAnswer),
            NumberAnswerDefinition definition => new NumberQuestionEditorResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Number,
                tags,
                question.Explanation,
                status,
                question.CreatedByUserId,
                question.CreatedAt,
                question.UpdatedAt,
                definition.CorrectAnswer),
            MatchingAnswerDefinition definition => new MatchingQuestionEditorResponse(
                question.Id,
                question.Text,
                question.ImageId,
                definition.Type.ToDto(),
                definition.Mode.ToDto(),
                tags,
                question.Explanation,
                status,
                question.CreatedByUserId,
                question.CreatedAt,
                question.UpdatedAt,
                ToMatchingItems(definition.LeftItems),
                ToMatchingItems(definition.RightItems),
                ToMatchingPairs(definition)),
            _ => throw Unsupported(question),
        };
    }

    public static QuestionPreviewResponse ToPreviewResponse(this Question question)
    {
        var tags = question.Tags.ToResponses();

        QuestionPreviewResponse response = question.AnswerDefinition switch
        {
            ChoiceAnswerDefinition definition => new ChoiceQuestionPreviewResponse(
                question.Id,
                question.Text,
                question.ImageId,
                definition.Type.ToDto(),
                definition.Mode.ToDto(),
                definition.EvaluationMode.ToDto(),
                tags),
            TextAnswerDefinition => new TextQuestionPreviewResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Text,
                tags),
            NumberAnswerDefinition => new NumberQuestionPreviewResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Number,
                tags),
            MatchingAnswerDefinition definition => new MatchingQuestionPreviewResponse(
                question.Id,
                question.Text,
                question.ImageId,
                definition.Type.ToDto(),
                definition.Mode.ToDto(),
                tags),
            _ => throw Unsupported(question),
        };

        return response with { Status = question.Status.ToDto() };
    }

    private static IReadOnlyList<AnswerOptionResultResponse> ToResultOptions(ChoiceAnswerDefinition definition) =>
        definition.Options.Select(option => new AnswerOptionResultResponse(
            option.Id,
            option.Text,
            option.ImageId,
            option.IsCorrect)).ToList();

    private static IReadOnlyList<MatchingItemResponse> ToMatchingItems(IEnumerable<MatchingItem> items) =>
        items.Select(item => new MatchingItemResponse(item.Id, item.Text, item.ImageId)).ToList();

    private static IReadOnlyList<MatchingPairDto> ToMatchingPairs(MatchingAnswerDefinition definition) =>
        definition.Pairs.Select(pair => new MatchingPairDto(pair.LeftId, pair.RightId)).ToList();

    private static NotSupportedException Unsupported(Question question) =>
        new($"Unsupported answer definition: {question.AnswerDefinition.GetType().Name}");
}
