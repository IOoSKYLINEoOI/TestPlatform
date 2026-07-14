namespace TestPlatform.Application.Common.Error;

public static class ErrorCodes
{
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";

    public const string ExamNotFound = "exam_not_found";

    public const string TestNotFound = "test_not_found";

    public const string QuestionNotFound = "question_not_found";
    public const string QuestionNotInExam = "question_not_in_exam";

    public const string TagNotFound = "tag_not_found";

    public const string AttemptNotFound = "attempt_not_found";
    public const string AttemptNotFinished = "attempt_not_finished";
}