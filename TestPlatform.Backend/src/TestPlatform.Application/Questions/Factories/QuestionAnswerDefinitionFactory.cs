using CSharpFunctionalExtensions;
using TestPlatform.Application.Questions.Mappers;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;
using TestPlatform.Core.Questions.AnswerDefinition;
using TestPlatform.Core.Questions.AnswerDefinition.Abstractions;

namespace TestPlatform.Application.Questions.Factories;

public static class QuestionAnswerDefinitionFactory
{
    public static Result<QuestionAnswerDefinition> Create(QuestionRequest request)
    {
        return request switch
        {
            ChoiceQuestionRequest choice =>
                CreateChoice(choice),

            TextQuestionRequest text =>
                CreateText(text),

            NumberQuestionRequest number =>
                CreateNumber(number),

            MatchingQuestionRequest matching =>
                CreateMatching(matching),

            _ => Result.Failure<QuestionAnswerDefinition>(
                "Unsupported request")
        };
    }

    private static Result<QuestionAnswerDefinition> CreateChoice(ChoiceQuestionRequest request)
    {
        var options = request.Options
            .Select(o => AnswerOption.Create(
                o.Text,
                o.IsCorrect,
                o.ImageId))
            .ToList();

        var combined = Result.Combine(options);

        if (combined.IsFailure)
        {
            return Result.Failure<QuestionAnswerDefinition>(combined.Error);
        }

        var definitionResult = ChoiceAnswerDefinition.Create(
            request.Mode.ToDomain(),
            request.EvaluationMode.ToDomain(),
            options.Select(x => x.Value));

        return definitionResult.IsFailure
            ? Result.Failure<QuestionAnswerDefinition>(definitionResult.Error)
            : Result.Success<QuestionAnswerDefinition>(definitionResult.Value);
    }

    private static Result<QuestionAnswerDefinition> CreateText(TextQuestionRequest request)
    {
        var definitionResult = TextAnswerDefinition.Create(request.CorrectAnswer);

        return definitionResult.IsFailure
            ? Result.Failure<QuestionAnswerDefinition>(definitionResult.Error)
            : Result.Success<QuestionAnswerDefinition>(definitionResult.Value);
    }

    private static Result<QuestionAnswerDefinition> CreateNumber(NumberQuestionRequest request)
    {
        var definitionResult = NumberAnswerDefinition.Create(request.CorrectAnswer);

        return definitionResult.IsFailure
            ? Result.Failure<QuestionAnswerDefinition>(definitionResult.Error)
            : Result.Success<QuestionAnswerDefinition>(definitionResult.Value);
    }

    private static Result<QuestionAnswerDefinition> CreateMatching(MatchingQuestionRequest request)
    {
        var leftItems = request.LeftItems
            .Select(i => MatchingItem.Create(
                i.Id,
                i.Text,
                i.ImageId))
            .ToList();

        var rightItems = request.RightItems
            .Select(i => MatchingItem.Create(
                i.Id,
                i.Text,
                i.ImageId))
            .ToList();

        var leftResult = Result.Combine(leftItems);
        if (leftResult.IsFailure)
        {
            return Result.Failure<QuestionAnswerDefinition>(leftResult.Error);
        }

        var rightResult = Result.Combine(rightItems);
        if (rightResult.IsFailure)
        {
            return Result.Failure<QuestionAnswerDefinition>(rightResult.Error);
        }

        var pairs = request.Pairs
            .Select(p => new MatchingPair(p.LeftId, p.RightId))
            .ToList();

        var definitionResult = MatchingAnswerDefinition.Create(
            request.EvaluationMode.ToDomain(),
            leftItems.Select(x => x.Value),
            rightItems.Select(x => x.Value),
            pairs);

        return definitionResult.IsFailure
            ? Result.Failure<QuestionAnswerDefinition>(definitionResult.Error)
            : Result.Success<QuestionAnswerDefinition>(definitionResult.Value);
    }
}
