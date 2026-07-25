using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Application.Questions.Mappers;

public static class QuestionStatusMapping
{
    public static QuestionStatusDto ToDto(this QuestionStatus status) => status switch
    {
        QuestionStatus.Draft => QuestionStatusDto.Draft,
        QuestionStatus.Published => QuestionStatusDto.Published,
        QuestionStatus.Archived => QuestionStatusDto.Archived,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };
}
