namespace TestPlatform.Core.Attempts.Enums;

public enum AttemptStatus
{
    /// <summary>
    /// Попытка создана и находится в процессе прохождения.
    /// </summary>
    STARTED = 1,

    /// <summary>
    /// Тест завершён пользователем с сохранением результата.
    /// </summary>
    FINISHED = 2,

    /// <summary>
    /// Попытка завершена автоматически по истечении времени.
    /// </summary>
    EXPIRED = 3,

    /// <summary>
    /// Пользователь покинул тест, не завершив его.
    /// </summary>
    ABANDONED = 4,

    /// <summary>
    /// Попытка была отменена системой или администратором.
    /// </summary>
    CANCELLED = 5,

    /// <summary>
    /// Попытка создана, но не начата.
    /// </summary>
    NOT_STARTED = 6,
}