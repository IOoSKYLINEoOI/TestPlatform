using System.Text.Json.Serialization;

namespace TestPlatform.Contracts.Tests.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TestStatusDto
{
    Draft = 1,
    Published = 2,
    Archived = 3,
}
