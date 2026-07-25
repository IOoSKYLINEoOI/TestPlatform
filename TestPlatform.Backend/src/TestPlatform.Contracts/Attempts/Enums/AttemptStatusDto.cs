using System.Text.Json.Serialization;

namespace TestPlatform.Contracts.Attempts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttemptStatusDto
{
    [JsonStringEnumMemberName("started")]
    STARTED = 1,

    [JsonStringEnumMemberName("finished")]
    FINISHED = 2,

    [JsonStringEnumMemberName("expired")]
    EXPIRED = 3,

    [JsonStringEnumMemberName("abandoned")]
    ABANDONED = 4,

    [JsonStringEnumMemberName("cancelled")]
    CANCELLED = 5,

    [JsonStringEnumMemberName("notStarted")]
    NOT_STARTED = 6,
}
