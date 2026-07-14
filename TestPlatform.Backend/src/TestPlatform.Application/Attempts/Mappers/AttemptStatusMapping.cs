using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Mappers;

public static class AttemptStatusMapping
{
    public static AttemptStatusDto ToDto(this AttemptStatus status)
    {
        return status switch
        {
            AttemptStatus.STARTED => AttemptStatusDto.STARTED,
            AttemptStatus.FINISHED => AttemptStatusDto.FINISHED,
            AttemptStatus.EXPIRED => AttemptStatusDto.EXPIRED,
            AttemptStatus.ABANDONED => AttemptStatusDto.ABANDONED,
            AttemptStatus.CANCELLED => AttemptStatusDto.CANCELLED,
            AttemptStatus.NOT_STARTED => AttemptStatusDto.NOT_STARTED,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static AttemptStatus ToDomain(this AttemptStatusDto statusDto)
    {
        return statusDto switch
        {
            AttemptStatusDto.STARTED => AttemptStatus.STARTED,
            AttemptStatusDto.FINISHED => AttemptStatus.FINISHED,
            AttemptStatusDto.EXPIRED => AttemptStatus.EXPIRED,
            AttemptStatusDto.ABANDONED => AttemptStatus.ABANDONED,
            AttemptStatusDto.CANCELLED => AttemptStatus.CANCELLED,
            AttemptStatusDto.NOT_STARTED => AttemptStatus.NOT_STARTED,
            _ => throw new ArgumentOutOfRangeException(nameof(statusDto), statusDto, null)
        };
    }
}