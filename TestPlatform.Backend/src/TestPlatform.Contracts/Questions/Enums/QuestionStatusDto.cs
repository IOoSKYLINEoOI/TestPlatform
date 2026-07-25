using System.Text.Json.Serialization;

namespace TestPlatform.Contracts.Questions.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuestionStatusDto
{
    Draft = 1,
    Published = 2,
    Archived = 3,
}
