using Microsoft.AspNetCore.Mvc;
using TestPlatform.Application.Common.Error;

namespace TestPlatform.Api.Common;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToErrorResult(string error)
    {
        int status = error switch
        {
            ErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCodes.Forbidden or
            ErrorCodes.FileForbidden => StatusCodes.Status403Forbidden,
            ErrorCodes.ExamNotFound or
            ErrorCodes.TestNotFound or
            ErrorCodes.QuestionNotFound or
            ErrorCodes.TagNotFound or
            ErrorCodes.AttemptNotFound or
            ErrorCodes.FileNotFound => StatusCodes.Status404NotFound,
            ErrorCodes.TagAlreadyExists or
            ErrorCodes.TagInUse or
            ErrorCodes.IdentityUsernameAlreadyExists or
            ErrorCodes.IdentityEmployeeNumberAlreadyExists or
            ErrorCodes.AttemptNotFinished or
            ErrorCodes.AttemptReviewNotAvailable or
            ErrorCodes.ExamAttemptsLimitReached => StatusCodes.Status409Conflict,
            ErrorCodes.FileInUse => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        return Problem(
            statusCode: status,
            title: GetTitle(status),
            extensions: new Dictionary<string, object?> { ["code"] = error });
    }

    private static string GetTitle(int status) => status switch
    {
        StatusCodes.Status401Unauthorized => "Authentication is required.",
        StatusCodes.Status403Forbidden => "Access is forbidden.",
        StatusCodes.Status404NotFound => "The requested resource was not found.",
        StatusCodes.Status409Conflict => "The request conflicts with the current resource state.",
        _ => "The request is invalid.",
    };
}
