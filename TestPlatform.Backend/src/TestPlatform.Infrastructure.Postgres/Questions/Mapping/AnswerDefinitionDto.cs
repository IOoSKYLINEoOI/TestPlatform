using System.Text.Json;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Infrastructure.Postgres.Questions.Mapping;

public class AnswerDefinitionDto
{
    public QuestionType Type { get; set; }

    public JsonElement Data { get; set; }
}