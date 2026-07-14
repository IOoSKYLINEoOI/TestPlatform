using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Application.Questions.Mappers;

public static class EvaluationModeMapping
{
    public static EvaluationMode ToDomain(this EvaluationModeDto mode)
    {
        return mode switch
        {
            EvaluationModeDto.Strict => EvaluationMode.Strict,
            EvaluationModeDto.Partial => EvaluationMode.Partial,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public static EvaluationModeDto ToDto(this EvaluationMode mode)
    {
        return mode switch
        {
            EvaluationMode.Strict => EvaluationModeDto.Strict,
            EvaluationMode.Partial => EvaluationModeDto.Partial,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}