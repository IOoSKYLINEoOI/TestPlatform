using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestPlatform.Infrastructure.Postgres.Questions.Mapping;

public class AnswerDefinitionDto
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
}
