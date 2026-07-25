namespace TestPlatform.Application.Common.Error;

public static class ErrorCodes
{
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";

    public const string ExamNotFound = TestPlatform.Core.Common.Errors.ExamErrors.NotFound;
    public const string ExamAttemptsLimitReached = TestPlatform.Core.Common.Errors.ExamErrors.AttemptsLimitReached;

    public const string TestNotFound = TestPlatform.Core.Common.Errors.TestErrors.NotFound;

    public const string QuestionNotFound = TestPlatform.Core.Common.Errors.QuestionErrors.NotFound;
    public const string QuestionNotInExam = TestPlatform.Core.Common.Errors.ExamErrors.QuestionNotInExam;

    public const string TagNotFound = TestPlatform.Core.Common.Errors.TagErrors.NotFound;
    public const string TagAlreadyExists = TestPlatform.Core.Common.Errors.TagErrors.AlreadyExists;
    public const string TagInUse = TestPlatform.Core.Common.Errors.TagErrors.InUse;
    public const string TagMergeSameTarget = TestPlatform.Core.Common.Errors.TagErrors.MergeSameTarget;

    public const string AttemptNotFound = TestPlatform.Core.Common.Errors.AttemptErrors.NotFound;
    public const string AttemptNotFinished = TestPlatform.Core.Common.Errors.AttemptErrors.NotFinished;
    public const string AttemptReviewNotAvailable = TestPlatform.Core.Common.Errors.AttemptErrors.ReviewNotAvailable;

    public const string FileNotFound = TestPlatform.Core.Common.Errors.FileErrors.NotFound;
    public const string FileForbidden = TestPlatform.Core.Common.Errors.FileErrors.Forbidden;
    public const string FileInUse = "file.in_use";

    public const string IdentityUsernameAlreadyExists =
        TestPlatform.Application.Users.IdentityAccountErrors.UsernameAlreadyExists;
    public const string IdentityEmployeeNumberAlreadyExists =
        TestPlatform.Application.Users.IdentityAccountErrors.EmployeeNumberAlreadyExists;
}
