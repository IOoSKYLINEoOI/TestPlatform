using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Mappers;

public static class AttemptTypeMapping
{
    public static AttemptTypeDto ToDto(this AttemptType type)
    {
        return type switch
        {
            AttemptType.Test => AttemptTypeDto.Test,
            AttemptType.Exam => AttemptTypeDto.Exam,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static AttemptType ToDomain(this AttemptTypeDto typeDto)
    {
        return typeDto switch
        {
            AttemptTypeDto.Test => AttemptType.Test,
            AttemptTypeDto.Exam => AttemptType.Exam,
            _ => throw new ArgumentOutOfRangeException(nameof(typeDto), typeDto, null)
        };
    }
}
