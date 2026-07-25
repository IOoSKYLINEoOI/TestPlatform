namespace TestPlatform.Core.Common.Errors;

public static class AttemptErrors
{
    public const string NotFound = "attempt.not_found";
    public const string NotFinished = "attempt.not_finished";
    public const string ReviewNotAvailable = "attempt.review_not_available";
    public const string RequestIdRequired = "attempt.request_id_required";
    public const string RequestIdConflict = "attempt.request_id_conflict";
    public const string QuestionNotInAttempt = "attempt.question_not_in_attempt";
    public const string AttemptsLimitReached = "exam.attempts_limit_reached";
}
