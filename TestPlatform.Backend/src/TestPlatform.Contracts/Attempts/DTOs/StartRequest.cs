using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record StartRequest(AttemptTypeDto Type, Guid SourceId, Guid RequestId);
