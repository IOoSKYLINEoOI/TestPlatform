using System.Text.Json.Serialization;

namespace TestPlatform.Contracts.Attempts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttemptTypeDto
{
    Test,
    Exam,
}
