using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Extensions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Attempts.Services;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions.Extensions;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Attempts.DTOs;

namespace TestPlatform.Application.Attempts.Features.GetByIdAttemptDetailsQuery;

public record GetByIdAttemptDetailsQuery(Guid Id) : IQuery;

public class GetByIdAttemptDetailsHandler(
    IAttemptsReadDbContext attemptsDbContext,
    AttemptQuestionLoader questionLoader,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<GetByIdAttemptDetailsQuery, AttemptDetailsResponse>
{
    public async Task<Result<AttemptDetailsResponse>> Handle(
        GetByIdAttemptDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.User;
        if (user is null)
        {
            return Result.Failure<AttemptDetailsResponse>(ErrorCodes.Unauthorized);
        }

        var attempt = await attemptsDbContext.ReadAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == query.Id && (x.UserId == user.Id || user.IsAdmin),
                cancellationToken);

        if (attempt is null)
        {
            return Result.Failure<AttemptDetailsResponse>(ErrorCodes.AttemptNotFound);
        }

        var questionsResult = await questionLoader.LoadAsync(
            attempt.QuestionSelections,
            cancellationToken);

        if (questionsResult.IsFailure)
        {
            return Result.Failure<AttemptDetailsResponse>(questionsResult.Error);
        }

        var answers = attempt.AttemptAnswers
            .ToDictionary(
                x => x.QuestionId,
                x => x.ToResponse());

        if (attempt.AttemptResult is null)
        {
            return Result.Failure<AttemptDetailsResponse>(
                ErrorCodes.AttemptNotFinished);
        }

        if (attempt.ReviewAvailableAt.HasValue && DateTime.UtcNow < attempt.ReviewAvailableAt.Value)
        {
            return Result.Failure<AttemptDetailsResponse>("attempt.review_not_available");
        }

        var percentage = attempt.Type == TestPlatform.Core.Attempts.Enums.AttemptType.Test
            ? (double)attempt.AttemptResult.CorrectAnswers / attempt.TotalQuestions * 100
            : (double)(attempt.AttemptResult.EarnedPoints / attempt.TotalMaxScore * 100);

        AttemptDetailsResponse response = attempt.Type switch
        {
            TestPlatform.Core.Attempts.Enums.AttemptType.Test => new TestAttemptDetailsResponse(
                attempt.Id,
                attempt.Status.ToDto(),
                attempt.AttemptResult.CorrectAnswers,
                attempt.TotalQuestions,
                percentage,
                questionsResult.Value
                    .OrderBy(x => x.Order)
                    .Select(x => new TestAttemptQuestionDetailsResponse(
                        x.Order,
                        x.Question.ToAttemptResultResponse(),
                        answers.GetValueOrDefault(x.Question.Id)))
                    .ToList()),
            TestPlatform.Core.Attempts.Enums.AttemptType.Exam => new ExamAttemptDetailsResponse(
                attempt.Id,
                attempt.Status.ToDto(),
                attempt.AttemptResult.CorrectAnswers,
                attempt.TotalQuestions,
                percentage,
                attempt.AttemptResult.EarnedPoints,
                attempt.TotalMaxScore,
                attempt.AttemptResult.Passed ?? false,
                questionsResult.Value
                    .OrderBy(x => x.Order)
                    .Select(x => new ExamAttemptQuestionDetailsResponse(
                        x.Order,
                        x.Score,
                        x.Question.ToAttemptResultResponse(),
                        answers.GetValueOrDefault(x.Question.Id)))
                    .ToList()),
            _ => throw new InvalidOperationException($"Unsupported attempt type: {attempt.Type}."),
        };

        return Result.Success(response);
    }
}
