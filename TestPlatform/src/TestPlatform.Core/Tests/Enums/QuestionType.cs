namespace TestPlatform.Core.Tests.Enums;

public enum QuestionType
{
    /// <summary>
    /// Один правильный вариант ответа
    /// </summary>
    SingleChoice = 1,

    /// <summary>
    /// Несколько правильных вариантов ответа
    /// </summary>
    MultipleChoice = 2,

    /// <summary>
    /// Текстовый ответ
    /// </summary>
    Text = 3,

    /// <summary>
    /// Числовой ответ
    /// </summary>
    Number = 4,

    /// <summary>
    /// Соотношение
    /// </summary>
    Matching = 5,
}