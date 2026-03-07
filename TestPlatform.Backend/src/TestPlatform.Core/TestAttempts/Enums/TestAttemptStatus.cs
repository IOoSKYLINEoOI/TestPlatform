namespace TestPlatform.Core.TestAttempts.Enums;

public enum TestAttemptStatus
{
    /// <summary>
    /// Попытка создана и находится в процессе прохождения.
    /// </summary>
    Started = 1,

    /// <summary>
    /// Тест завершён пользователем с сохранением результата.
    /// </summary>
    Finished = 2,

    /// <summary>
    /// Попытка завершена автоматически по истечении времени.
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Пользователь покинул тест, не завершив его.
    /// </summary>
    Abandoned = 4,

    /// <summary>
    /// Попытка была отменена системой или администратором.
    /// </summary>
    Cancelled = 5,
}