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
        catch (Exception)
        {
            return Result.Failure<string>("persistence.answer_definition.serialization_failed");
        }
    }

    public Result<QuestionAnswerDefinition> Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.empty_json");
        }

        AnswerDefinitionDto? dto;

        try
        {
            dto = JsonSerializer.Deserialize<AnswerDefinitionDto>(json);
        }
        catch (JsonException)
        {
            return Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.invalid_json");
        }

        if (dto is null)
        {
            return Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.deserialization_failed");
        }

        return dto.Type switch
        {
            "choice" => MapChoice(dto.Data),
            "text" => MapText(dto.Data),
            "number" => MapNumber(dto.Data),
            "matching" => MapMatching(dto.Data),

            _ => Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.unsupported_type")
        };
    }

    private string SerializeChoice(ChoiceAnswerDefinition def)
    {
        var obj = new
        {
            schemaVersion = 1,
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
            schemaVersion = 1,
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
            schemaVersion = 1,
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
            schemaVersion = 1,
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
                    imageId = x.ImageId,
                }),

                right = def.RightItems.Select(x => new
                {
                    id = x.Id,
                    text = x.Text,
                    imageId = x.ImageId,
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

        var options = new List<AnswerOption>();
        foreach (var option in data.GetProperty("options").EnumerateArray())
        {
            if (!TryGetGuid(option, "id", out var id))
            {
                return Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.invalid_option_id");
            }

            var optionResult = AnswerOption.Create(
                id,
                option.GetProperty("text").GetString() ?? string.Empty,
                option.GetProperty("isCorrect").GetBoolean(),
                TryGetImageId(option));

            if (optionResult.IsFailure)
            {
                return Result.Failure<QuestionAnswerDefinition>(optionResult.Error);
            }

            options.Add(optionResult.Value);
        }

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
        {
            return Result.Failure<QuestionAnswerDefinition>(result.Error);
        }

        return Result.Success<QuestionAnswerDefinition>(result.Value);
    }

    private Result<QuestionAnswerDefinition> MapText(JsonElement data)
    {
        if (!data.TryGetProperty("answer", out var answerProp))
        {
            return Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.answer_missing");
        }

        var answer = answerProp.GetString();

        if (string.IsNullOrWhiteSpace(answer))
        {
            return Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.text_answer_empty");
        }

        var result = TextAnswerDefinition.Create(answer);

        if (result.IsFailure)
        {
            return Result.Failure<QuestionAnswerDefinition>(result.Error);
        }

        return Result.Success<QuestionAnswerDefinition>(result.Value);
    }

    private Result<QuestionAnswerDefinition> MapNumber(JsonElement data)
    {
        if (!data.TryGetProperty("answer", out var answerProp))
        {
            return Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.answer_missing");
        }

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
                {
                    return Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.invalid_decimal");
                }
            }
            else
            {
                return Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.invalid_number_type");
            }
        }
        catch (Exception)
        {
            return Result.Failure<QuestionAnswerDefinition>("persistence.answer_definition.number_parsing_failed");
        }

        var result = NumberAnswerDefinition.Create(value);

        if (result.IsFailure)
        {
            return Result.Failure<QuestionAnswerDefinition>(result.Error);
        }

        return Result.Success<QuestionAnswerDefinition>(result.Value);
    }

    private Result<QuestionAnswerDefinition> MapMatching(JsonElement data)
    {
        var leftResult = ParseItems(data.GetProperty("left"));
        if (leftResult.IsFailure)
        {
            return Result.Failure<QuestionAnswerDefinition>(leftResult.Error);
        }

        var rightResult = ParseItems(data.GetProperty("right"));
        if (rightResult.IsFailure)
        {
            return Result.Failure<QuestionAnswerDefinition>(rightResult.Error);
        }

        var pairsResult = ParsePairs(data.GetProperty("pairs"));
        if (pairsResult.IsFailure)
        {
            return Result.Failure<QuestionAnswerDefinition>(pairsResult.Error);
        }

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
        {
            return Result.Failure<QuestionAnswerDefinition>(result.Error);
        }

        return Result.Success<QuestionAnswerDefinition>(result.Value);
    }

    private Result<List<MatchingItem>> ParseItems(JsonElement array)
    {
        var list = new List<MatchingItem>();

        foreach (var item in array.EnumerateArray())
        {
            if (!item.TryGetProperty("text", out var textProp))
            {
                return Result.Failure<List<MatchingItem>>("persistence.answer_definition.item_text_missing");
            }

            var text = textProp.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return Result.Failure<List<MatchingItem>>("persistence.answer_definition.item_text_empty");
            }

            var imageId = TryGetImageId(item);

            if (!TryGetGuid(item, "id", out var id))
            {
                return Result.Failure<List<MatchingItem>>("persistence.answer_definition.invalid_matching_item_id");
            }

            var result = MatchingItem.Create(id, text, imageId);

            if (result.IsFailure)
            {
                return Result.Failure<List<MatchingItem>>(result.Error);
            }

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
            {
                return Result.Failure<List<MatchingPair>>("persistence.answer_definition.left_id_missing");
            }

            if (!item.TryGetProperty("rightId", out var rightProp))
            {
                return Result.Failure<List<MatchingPair>>("persistence.answer_definition.right_id_missing");
            }

            if (!TryReadGuid(leftProp, out var leftId))
            {
                return Result.Failure<List<MatchingPair>>("persistence.answer_definition.invalid_left_id");
            }

            if (!TryReadGuid(rightProp, out var rightId))
            {
                return Result.Failure<List<MatchingPair>>("persistence.answer_definition.invalid_right_id");
            }

            list.Add(new MatchingPair(leftId, rightId));
        }

        return Result.Success(list);
    }
    private static Guid? TryGetImageId(JsonElement element)
    {
        if (!element.TryGetProperty("imageId", out var imageIdProperty))
        {
            return null;
        }

        return TryReadGuid(imageIdProperty, out var imageId) ? imageId : null;
    }

    private static bool TryGetGuid(JsonElement element, string propertyName, out Guid id)
    {
        id = Guid.Empty;
        return element.TryGetProperty(propertyName, out var property) && TryReadGuid(property, out id);
    }

    private static bool TryReadGuid(JsonElement property, out Guid id)
    {
        id = Guid.Empty;
        return property.ValueKind == JsonValueKind.String && Guid.TryParse(property.GetString(), out id);
    }
}
