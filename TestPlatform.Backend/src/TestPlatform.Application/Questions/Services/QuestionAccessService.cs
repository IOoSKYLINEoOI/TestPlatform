using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Users;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions.Services;

public sealed class QuestionAccessService(
    IQuestionsRepository questionsRepository,
    ICurrentUserAccessor currentUserAccessor,
    ILogger<QuestionAccessService> logger)
    : IAccessService<Question>
{
    public async Task<Result<Question>> GetForModifyAsync(Guid id, CancellationToken ct)
    {
        var currentUser = currentUserAccessor.User;
        if (currentUser is null)
        {
            return Result.Failure<Question>(ErrorCodes.Unauthorized);
        }

        var question = await questionsRepository.GetByIdAsync(id, ct);
        if (question is null)
        {
            return Result.Failure<Question>(ErrorCodes.QuestionNotFound);
        }

        if (question.CreatedByUserId != currentUser.Id && !currentUser.IsAdmin)
        {
            logger.LogWarning(
                "User {UserId} has no rights to modify question {QuestionId}",
                currentUser.Id,
                id);
            return Result.Failure<Question>(ErrorCodes.Forbidden);
        }

        return Result.Success(question);
    }
}
