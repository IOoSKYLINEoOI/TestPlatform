using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Extensions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Attempts.Services.SourceService;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions.Extensions;
using TestPlatform.Contracts.Attempts.DTOs;

namespace TestPlatform.Application.Attempts.Features.GetByIdAttemptDetailsQuery;

public record GetByIdAttemptDetailsQuery(Guid Id) : IQuery;

public class GetByIdAttemptDetailsHandler(
    IAttemptsReadDbContext attemptsDbContext,
    AttemptSourceResolver resolver)
    : IQueryHandler<GetByIdAttemptDetailsQuery, AttemptDetailsResponse>
{
    public async Task<Result<AttemptDetailsResponse>> Handle(
        GetByIdAttemptDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var attempt = await attemptsDbContext.ReadAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

        if (attempt is null)
            return Result.Failure<AttemptDetailsResponse>(ErrorCodes.AttemptNotFound);

        var sourceResult = await resolver.GetSourceAsync(
            attempt.Type,
            attempt.SourceId,
            cancellationToken);

        if (sourceResult.IsFailure)
            return Result.Failure<AttemptDetailsResponse>(sourceResult.Error);

        var answers = attempt.AttemptAnswers
            .ToDictionary(
                x => x.QuestionId,
                x => x.ToResponse());

        var questions = sourceResult.Value.Questions
            .OrderBy(x => x.Order)
            .Select(x => new AttemptQuestionDetailsResponse(
                x.Order,
                x.Score,
                x.Question.ToResultResponse(),
                answers.GetValueOrDefault(x.Question.Id)))
            .ToList();

        if (attempt.AttemptResult is null)
        {
            return Result.Failure<AttemptDetailsResponse>(
                ErrorCodes.AttemptNotFinished);
        }

        return Result.Success(
            new AttemptDetailsResponse(
                attempt.Id,
                attempt.Status.ToDto(),
                attempt.AttemptResult.CorrectAnswers,
                attempt.TotalQuestions,
                attempt.AttemptResult.EarnedPoints,
                attempt.TotalMaxScore,
                questions));
    }
}
