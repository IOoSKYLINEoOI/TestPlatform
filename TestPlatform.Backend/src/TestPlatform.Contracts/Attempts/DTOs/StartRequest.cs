using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record StartRequest(Guid UserId, AttemptTypeDto Type, Guid SourceId);