using TestPlatform.Application.Questions.Mappers;
using TestPlatform.Application.Tags.Extensions;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;
using TestPlatform.Contracts.Questions.DTOs.Preview;
using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Core.Questions;
using TestPlatform.Core.Questions.AnswerDefinition;

namespace TestPlatform.Application.Questions.Extensions;

public static class QuestionMappingExtensions
{
    public static QuestionResponse ToResponse(this Question question)
    {
        var tags = question.Tags.ToResponses();

        return question.AnswerDefinition switch
        {
            ChoiceAnswerDefinition c => new ChoiceQuestionResponse(
                question.Id,
                question.Text,
                question.ImageId,
                c.Type.ToDto(),
                c.Mode.ToDto(),
                c.EvaluationMode.ToDto(),
                tags,
                c.Options.Select(o => new AnswerOptionResponse(
                    o.Id,
                    o.Text,
                    o.ImageId)).ToList()),

            TextAnswerDefinition _ => new TextQuestionResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Text,
                tags),

            NumberAnswerDefinition _ => new NumberQuestionResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Number,
                tags),

            MatchingAnswerDefinition m => new MatchingQuestionResponse(
                question.Id,
                question.Text,
                question.ImageId,
                m.Type.ToDto(),
                m.Mode.ToDto(),
                tags,
                m.LeftItems.Select(o => new MatchingItemResponse(
                    o.Id,
                    o.Text,
                    o.ImageId)).ToList(),
                m.RightItems.Select(o => new MatchingItemResponse(
                    o.Id,
                    o.Text,
                    o.ImageId)).ToList()),

            _ => throw new NotSupportedException(
                     $"Unsupported answer definition: {question.AnswerDefinition.GetType().Name}")
        };
    }

    public static QuestionResultResponse ToResultResponse(this Question question)
    {
        var tags = question.Tags.ToResponses();

        return question.AnswerDefinition switch
        {
            ChoiceAnswerDefinition c => new ChoiceQuestionResultResponse(
                question.Id,
                question.Text,
                question.ImageId,
                c.Type.ToDto(),
                c.Mode.ToDto(),
                c.EvaluationMode.ToDto(),
                tags,
                c.Options.Select(o => new AnswerOptionResultResponse(
                    o.Id,
                    o.Text,
                    o.ImageId,
                    o.IsCorrect)).ToList()),

            TextAnswerDefinition t => new TextQuestionResultResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Text,
                t.CorrectAnswer,
                tags),

            NumberAnswerDefinition n => new NumberQuestionResultResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Number,
                n.CorrectAnswer,
                tags),

            MatchingAnswerDefinition m => new MatchingQuestionResultResponse(
                question.Id,
                question.Text,
                question.ImageId,
                m.Type.ToDto(),
                m.Mode.ToDto(),
                tags,
                m.LeftItems.Select(o => new MatchingItemResponse(
                    o.Id,
                    o.Text,
                    o.ImageId)).ToList(),
                m.RightItems.Select(o => new MatchingItemResponse(
                    o.Id,
                    o.Text,
                    o.ImageId)).ToList(),
                m.Pairs.Select(p => new MatchingPairDto(
                    p.LeftId,
                    p.RightId)).ToList()),

            _ => throw new NotSupportedException(
                $"Unsupported answer definition: {question.AnswerDefinition.GetType().Name}")
        };
    }

    public static QuestionPreviewResponse ToPreviewResponse(this Question question)
    {
        var tags = question.Tags.ToResponses();

        return question.AnswerDefinition switch
        {
            ChoiceAnswerDefinition c => new ChoiceQuestionPreviewResponse(
                question.Id,
                question.Text,
                question.ImageId,
                c.Type.ToDto(),
                c.Mode.ToDto(),
                c.EvaluationMode.ToDto(),
                tags),

            TextAnswerDefinition _ => new TextQuestionPreviewResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Text,
                tags),

            NumberAnswerDefinition _ => new NumberQuestionPreviewResponse(
                question.Id,
                question.Text,
                question.ImageId,
                QuestionTypeDto.Number,
                tags),

            MatchingAnswerDefinition m => new MatchingQuestionPreviewResponse(
                question.Id,
                question.Text,
                question.ImageId,
                m.Type.ToDto(),
                m.Mode.ToDto(),
                tags),

            _ => throw new NotSupportedException(
                $"Unsupported answer definition: {question.AnswerDefinition.GetType().Name}")
        };
    }
}