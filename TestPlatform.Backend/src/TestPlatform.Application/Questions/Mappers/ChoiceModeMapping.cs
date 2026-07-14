using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Application.Questions.Mappers;

public static class ChoiceModeMapping
{
    public static ChoiceMode ToDomain(this ChoiceModeDto mode)
    {
        return mode switch
        {
            ChoiceModeDto.Single => ChoiceMode.Single,
            ChoiceModeDto.Multiple => ChoiceMode.Multiple,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public static ChoiceModeDto ToDto(this ChoiceMode mode)
    {
        return mode switch
        {
            ChoiceMode.Single => ChoiceModeDto.Single,
            ChoiceMode.Multiple => ChoiceModeDto.Multiple,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}