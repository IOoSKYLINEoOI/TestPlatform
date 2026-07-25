using System.Text.Json.Serialization;

namespace TestPlatform.Contracts.Questions.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuestionTypeDto
{
    /// <summary>
    /// Выбор ответа
    /// </summary>
    Choice = 1,

    /// <summary>
    /// Текстовый ответ
    /// </summary>
    Text = 2,

    /// <summary>
    /// Числовой ответ
    /// </summary>
    Number = 3,

    /// <summary>
    /// Соотношение
    /// </summary>
    Matching = 4,
}
