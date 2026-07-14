using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TestPlatform.Core.Questions.AnswerDefinition.Abstractions;

namespace TestPlatform.Infrastructure.Postgres.Questions.Mapping;

public sealed class AnswerDefinitionValueConverter
    : ValueConverter<QuestionAnswerDefinition, string>
{
    private static readonly AnswerDefinitionMapper Mapper = new();

    public AnswerDefinitionValueConverter()
        : base(
            definition => Mapper.Serialize(definition).Value,
            json => Mapper.Deserialize(json).Value)
    {
    }
}