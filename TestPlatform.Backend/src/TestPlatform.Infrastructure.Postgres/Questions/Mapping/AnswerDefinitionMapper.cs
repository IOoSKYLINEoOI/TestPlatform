using System.Text.Json;
using CSharpFunctionalExtensions;
using TestPlatform.Core.Questions.AnswerDefinition;
using TestPlatform.Core.Questions.AnswerDefinition.Abstractions;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Infrastructure.Postgres.Questions.Mapping;

public class AnswerDefinitionMapper
{
    public Result<string> Serialize(QuestionAnswerDefinition definition)
    {
        try
        {
            var json = definition switch
            {
                ChoiceAnswerDefinition c => SerializeChoice(c),
                TextAnswerDefinition t => SerializeText(t),
                NumberAnswerDefinition n => SerializeNumber(n),
                MatchingAnswerDefinition m => SerializeMatching(m),
                _ => throw new NotSupportedException()
            };

            return Result.Success(json);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(ex.Message);
        }
    }

    public Result<QuestionAnswerDefinition> Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Result.Failure<QuestionAnswerDefinition>("JSON is empty");

        AnswerDefinitionDto? dto;

        try
        {
            dto = JsonSerializer.Deserialize<AnswerDefinitionDto>(json);
        }
        catch (JsonException ex)
        {
            return Result.Failure<QuestionAnswerDefinition>($"Invalid JSON: {ex.Message}");
        }

        if (dto is null)
            return Result.Failure<QuestionAnswerDefinition>("Failed to deserialize DTO");

        if (!Enum.IsDefined(typeof(QuestionType), dto.Type))
            return Result.Failure<QuestionAnswerDefinition>($"Unknown type: {dto.Type}");

        return dto.Type switch
        {
            QuestionType.Choice => MapChoice(dto.Data),
            QuestionType.Text => MapText(dto.Data),
            QuestionType.Number => MapNumber(dto.Data),
            QuestionType.Matching => MapMatching(dto.Data),

            _ => Result.Failure<QuestionAnswerDefinition>($"Unsupported type: {dto.Type}")
        };
    }

    private string SerializeChoice(ChoiceAnswerDefinition def)
    {
        var obj = new
        {
            type = "choice",
            data = new
            {
                mode = def.Mode switch
                {
                    ChoiceMode.Single => "single",
                    ChoiceMode.Multiple => "multiple",
                    _ => throw new NotSupportedException()
                },

                evaluationMode = def.EvaluationMode switch
                {
                    EvaluationMode.Strict => "strict",
                    EvaluationMode.Partial => "partial",
                    _ => throw new NotSupportedException()
                },

                options = def.Options.Select(o => new
                {
                    id = o.Id,
                    text = o.Text,
                    isCorrect = o.IsCorrect,
                    imageId = o.ImageId,
                }),
            },
        };

        return JsonSerializer.Serialize(obj);
    }

    private string SerializeText(TextAnswerDefinition def)
    {
        var obj = new
        {
            type = "text",
            data = new
            {
                answer = def.CorrectAnswer,
            },
        };

        return JsonSerializer.Serialize(obj);
    }

    private string SerializeNumber(NumberAnswerDefinition def)
    {
        var obj = new
        {
            type = "number",
            data = new
            {
                answer = decimal.Round(def.CorrectAnswer, 6, MidpointRounding.AwayFromZero),
            },
        };

        return JsonSerializer.Serialize(obj);
    }

    private string SerializeMatching(MatchingAnswerDefinition def)
    {
        var obj = new
        {
            type = "matching",
            data = new
            {
                mode = def.Mode switch
                {
                    EvaluationMode.Strict => "strict",
                    EvaluationMode.Partial => "partial",
                    _ => throw new NotSupportedException()
                },

                left = def.LeftItems.Select(x => new
                {
                    id = x.Id,
                    text = x.Text,
                }),

                right = def.RightItems.Select(x => new
                {
                    id = x.Id,
                    text = x.Text,
                }),

                pairs = def.Pairs.Select(p => new
                {
                    leftId = p.LeftId,
                    rightId = p.RightId,
                }),
            },
        };

        return JsonSerializer.Serialize(obj);
    }

    private Result<QuestionAnswerDefinition> MapChoice(JsonElement data)
    {
        var mode = data.GetProperty("mode").GetString();
        var evaluationMode = data.GetProperty("evaluationMode").GetString();

        var options = data.GetProperty("options")
            .EnumerateArray()
            .Select(x => AnswerOption.Create(
                x.GetProperty("text").GetString()!,
                x.GetProperty("isCorrect").GetBoolean(),
                TryGetImageId(x)).Value)
            .ToList();

        var modeDomain = mode switch
        {
            "single" => ChoiceMode.Single,
            "multiple" => ChoiceMode.Multiple,
            _ => throw new NotSupportedException()
        };

        var evalDomain = evaluationMode switch
        {
            "strict" => EvaluationMode.Strict,
            "partial" => EvaluationMode.Partial,
            _ => throw new NotSupportedException()
        };

        var result = ChoiceAnswerDefinition.Create(
            modeDomain,
            evalDomain,
            options);

        if (result.IsFailure)
            return Result.Failure<QuestionAnswerDefinition>(result.Error);

        return Result.Success<QuestionAnswerDefinition>(result.Value);
    }

    private Result<QuestionAnswerDefinition> MapText(JsonElement data)
    {
        if (!data.TryGetProperty("answer", out var answerProp))
            return Result.Failure<QuestionAnswerDefinition>("Missing 'answer' field");

        var answer = answerProp.GetString();

        if (string.IsNullOrWhiteSpace(answer))
            return Result.Failure<QuestionAnswerDefinition>("Text answer is empty");

        var result = TextAnswerDefinition.Create(answer);

        if (result.IsFailure)
            return Result.Failure<QuestionAnswerDefinition>(result.Error);

        return Result.Success<QuestionAnswerDefinition>(result.Value);
    }

    private Result<QuestionAnswerDefinition> MapNumber(JsonElement data)
    {
        if (!data.TryGetProperty("answer", out var answerProp))
            return Result.Failure<QuestionAnswerDefinition>("Missing 'answer' field");

        decimal value;

        try
        {
            if (answerProp.ValueKind == JsonValueKind.Number)
            {
                value = answerProp.GetDecimal();
            }
            else if (answerProp.ValueKind == JsonValueKind.String)
            {
                var str = answerProp.GetString();

                if (!decimal.TryParse(str, out value))
                    return Result.Failure<QuestionAnswerDefinition>("Invalid decimal format in 'answer'");
            }
            else
            {
                return Result.Failure<QuestionAnswerDefinition>("Invalid 'answer' type for number question");
            }
        }
        catch (Exception ex)
        {
            return Result.Failure<QuestionAnswerDefinition>($"Failed to parse number: {ex.Message}");
        }

        var result = NumberAnswerDefinition.Create(value);

        if (result.IsFailure)
            return Result.Failure<QuestionAnswerDefinition>(result.Error);

        return Result.Success<QuestionAnswerDefinition>(result.Value);
    }

    private Result<QuestionAnswerDefinition> MapMatching(JsonElement data)
    {
        var leftResult = ParseItems(data.GetProperty("left"));
        if (leftResult.IsFailure)
            return Result.Failure<QuestionAnswerDefinition>(leftResult.Error);

        var rightResult = ParseItems(data.GetProperty("right"));
        if (rightResult.IsFailure)
            return Result.Failure<QuestionAnswerDefinition>(rightResult.Error);

        var pairsResult = ParsePairs(data.GetProperty("pairs"));
        if (pairsResult.IsFailure)
            return Result.Failure<QuestionAnswerDefinition>(pairsResult.Error);

        var mode = data.GetProperty("mode").GetString();

        var evaluationMode = mode switch
        {
            "strict" => EvaluationMode.Strict,
            "partial" => EvaluationMode.Partial,
            _ => throw new NotSupportedException()
        };

        var result = MatchingAnswerDefinition.Create(
            evaluationMode,
            leftResult.Value,
            rightResult.Value,
            pairsResult.Value);

        if (result.IsFailure)
            return Result.Failure<QuestionAnswerDefinition>(result.Error);

        return Result.Success<QuestionAnswerDefinition>(result.Value);
    }

    private Result<List<MatchingItem>> ParseItems(JsonElement array)
    {
        var list = new List<MatchingItem>();

        foreach (var item in array.EnumerateArray())
        {
            if (!item.TryGetProperty("text", out var textProp))
                return Result.Failure<List<MatchingItem>>("Missing 'text' in item");

            var text = textProp.GetString();
            if (string.IsNullOrWhiteSpace(text))
                return Result.Failure<List<MatchingItem>>("Item text is empty");

            var imageId = TryGetImageId(item);

            var result = MatchingItem.Create(text, imageId);

            if (result.IsFailure)
                return Result.Failure<List<MatchingItem>>(result.Error);

            list.Add(result.Value);
        }

        return Result.Success(list);
    }

    private Result<List<MatchingPair>> ParsePairs(JsonElement array)
    {
        var list = new List<MatchingPair>();

        foreach (var item in array.EnumerateArray())
        {
            if (!item.TryGetProperty("leftId", out var leftProp))
                return Result.Failure<List<MatchingPair>>("Missing leftId");

            if (!item.TryGetProperty("rightId", out var rightProp))
                return Result.Failure<List<MatchingPair>>("Missing rightId");

            var leftStr = leftProp.GetString();
            var rightStr = rightProp.GetString();

            if (!Guid.TryParse(leftStr, out var leftId))
                return Result.Failure<List<MatchingPair>>("Invalid leftId GUID");

            if (!Guid.TryParse(rightStr, out var rightId))
                return Result.Failure<List<MatchingPair>>("Invalid rightId GUID");

            list.Add(new MatchingPair(leftId, rightId));
        }

        return Result.Success(list);
    }
    private static Guid? TryGetImageId(JsonElement element)
    {
        if (!element.TryGetProperty("imageId", out var imageIdProperty))
            return null;

        var value = imageIdProperty.GetString();
        return Guid.TryParse(value, out var imageId) ? imageId : null;
    }
}