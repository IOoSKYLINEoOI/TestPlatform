using System.Text.Json.Serialization;

namespace TestPlatform.Contracts.Questions.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChoiceModeDto
{
    Single,
    Multiple,
}
