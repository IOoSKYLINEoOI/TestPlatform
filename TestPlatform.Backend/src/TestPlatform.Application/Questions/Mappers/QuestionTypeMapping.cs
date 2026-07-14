using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Application.Questions.Mappers;

public static class QuestionTypeMapping
{
    public static QuestionTypeDto ToDto(this QuestionType type)
    {
        return type switch
        {
            QuestionType.Choice => QuestionTypeDto.Choice,
            QuestionType.Text => QuestionTypeDto.Text,
            QuestionType.Number => QuestionTypeDto.Number,
            QuestionType.Matching => QuestionTypeDto.Matching,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}