using CSharpFunctionalExtensions;
using TestPlatform.Core.Enums;

namespace TestPlatform.Core.Models.TestAttempt;

public class TestAttempt
{
    private TestAttempt(Guid id, Guid testId, int totalQuestions)
    {
        Id = id;
        TotalQuestions = totalQuestions;
        StartedAt = DateTime.UtcNow;
        TestId = testId;
    }

    public Guid Id { get; }
    public int TotalQuestions { get; }
    public int? CorrectAnswers { get; private set; }
    public TestAttemptStatus TestAttemptStatus { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public Guid TestId { get; }

    public double Score
        => TestAttemptStatus == TestAttemptStatus.Finished
            ? (double)CorrectAnswers / TotalQuestions : 0;

    public static Result<TestAttempt> Create(Guid testId, int totalQuestions)
    {
        if (totalQuestions <= 0)
            return Result.Failure<TestAttempt>("Количество вопросов должно быть больше 0.");

        var testAttempt = new TestAttempt(Guid.NewGuid(), testId, totalQuestions)
        {
            TestAttemptStatus = TestAttemptStatus.Started
        };

        return Result.Success(testAttempt);
    }

    public static TestAttempt FromPersistence(
        Guid id,
        int totalQuestions,
        int? correctAnswers,
        DateTime startedAt,
        DateTime? finishedAt,
        Guid testId,
        TestAttemptStatus testAttemptStatus)
    {
        var testAttempt = new TestAttempt(id, testId, totalQuestions)
        {
            CorrectAnswers = correctAnswers,
            StartedAt = startedAt,
            FinishedAt = finishedAt,
            TestAttemptStatus = testAttemptStatus
        };

        return testAttempt;
    }

    public Result Finish(int correctAnswers)
    {
        if (correctAnswers < 0 || correctAnswers > TotalQuestions)
            return Result.Failure<TestAttempt>("Некорректное количество правильных ответов.");
        
        if (!IsActive())
            return Result.Failure("Попытка не может быть завершена.");

        CorrectAnswers = correctAnswers;
        
        return Complete(TestAttemptStatus.Finished);
    }

    public Result Expired() => Complete(TestAttemptStatus.Expired);
    public Result Abandoned() => Complete(TestAttemptStatus.Abandoned);
    public Result Cancelled() => Complete(TestAttemptStatus.Cancelled);
    
    private bool IsActive()
    {
        return TestAttemptStatus == TestAttemptStatus.Started;
    }

    private Result Complete(TestAttemptStatus status)
    {
        if (!IsActive())
            return Result.Failure("Попытка уже завершена.");
        
        if (status == TestAttemptStatus.Finished && CorrectAnswers is null)
            return Result.Failure("Невозможно завершить попытку без результата.");

        TestAttemptStatus = status;
        FinishedAt = DateTime.UtcNow;
        
        return Result.Success();
    }
}