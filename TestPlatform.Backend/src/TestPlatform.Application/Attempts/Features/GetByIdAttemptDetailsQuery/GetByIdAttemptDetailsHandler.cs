using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Extensions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Attempts.Services;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions.Extensions;
using TestPlatform.Application.Users;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Tests;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Features.GetByIdAttemptDetailsQuery;

public record GetByIdAttemptDetailsQuery(Guid Id) : IQuery;

public class GetByIdAttemptDetailsHandler(
    IAttemptsReadDbContext attemptsDbContext,
    AttemptQuestionLoader questionLoader,
    IExamsReadDbContext examsDbContext,
    ITestsReadDbContext testsDbContext,
    IUsersReadDbContext usersDbContext,
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
            .Include(x => x.QuestionSelections)
            .Include(x => x.AttemptAnswers)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == query.Id && (x.UserId == user.Id || user.IsAdmin),
                cancellationToken);

        if (attempt is null)
        {
            return Result.Failure<AttemptDetailsResponse>(ErrorCodes.AttemptNotFound);
        }

        var selections = await ResolveQuestionSelectionsAsync(attempt, cancellationToken);
        var questionsResult = await questionLoader.LoadAsync(
            selections,
            cancellationToken);

        if (questionsResult.IsFailure)
        {
            return Result.Failure<AttemptDetailsResponse>(questionsResult.Error);
        }

        var attemptAnswers = attempt.AttemptAnswers
            .ToDictionary(x => x.QuestionId);
        var answers = attemptAnswers
            .ToDictionary(
                x => x.Key,
                x => x.Value.ToResponse());

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
        var sourceTitle = attempt.Type == TestPlatform.Core.Attempts.Enums.AttemptType.Test
            ? await testsDbContext.ReadTests.Where(x => x.Id == attempt.SourceId).Select(x => x.Title).FirstOrDefaultAsync(cancellationToken)
            : await examsDbContext.ReadExams.Where(x => x.Id == attempt.SourceId).Select(x => x.Title).FirstOrDefaultAsync(cancellationToken);
        var employeeNumber = await usersDbContext.ReadUsers
            .Where(x => x.Id == attempt.UserId)
            .Select(x => x.EmployeeNumber)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        AttemptDetailsResponse response = attempt.Type switch
        {
            TestPlatform.Core.Attempts.Enums.AttemptType.Test => new TestAttemptDetailsResponse(
                attempt.Id,
                attempt.AttemptNumber,
                attempt.SourceId,
                sourceTitle ?? string.Empty,
                attempt.UserId,
                employeeNumber,
                attempt.StartedAt,
                attempt.FinishedAt,
                attempt.Status.ToDto(),
                attempt.AttemptResult.CorrectAnswers,
                attempt.TotalQuestions,
                percentage,
                questionsResult.Value
                    .OrderBy(x => x.Order)
                    .Select(x => new TestAttemptQuestionDetailsResponse(
                        x.Order,
                        IsCorrect(x, attemptAnswers.GetValueOrDefault(x.Question.Id)),
                        EarnedScore(x, attemptAnswers.GetValueOrDefault(x.Question.Id)),
                        x.Score,
                        x.Question.ToAttemptResultResponse(),
                        answers.GetValueOrDefault(x.Question.Id)))
                    .ToList()),
            TestPlatform.Core.Attempts.Enums.AttemptType.Exam => new ExamAttemptDetailsResponse(
                attempt.Id,
                attempt.AttemptNumber,
                attempt.SourceId,
                sourceTitle ?? string.Empty,
                attempt.UserId,
                employeeNumber,
                attempt.StartedAt,
                attempt.FinishedAt,
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
                        IsCorrect(x, attemptAnswers.GetValueOrDefault(x.Question.Id)),
                        EarnedScore(x, attemptAnswers.GetValueOrDefault(x.Question.Id)),
                        x.Score,
                        x.Question.ToAttemptResultResponse(),
                        answers.GetValueOrDefault(x.Question.Id)))
                    .ToList()),
            _ => throw new InvalidOperationException($"Unsupported attempt type: {attempt.Type}."),
        };

        return Result.Success(response);
    }

    private async Task<IReadOnlyCollection<AttemptQuestionSelection>> ResolveQuestionSelectionsAsync(
        Attempt attempt,
        CancellationToken cancellationToken)
    {
        if (attempt.QuestionSelections.Count > 0)
        {
            return attempt.QuestionSelections;
        }

        if (attempt.Type == AttemptType.Test)
        {
            var test = await testsDbContext.ReadTests
                .Include(x => x.Questions)
                .FirstOrDefaultAsync(x => x.Id == attempt.SourceId, cancellationToken);

            return test?.Questions
                .OrderBy(x => x.Order)
                .Select(x => new AttemptQuestionSelection(x.QuestionId, x.Order, 1m))
                .ToList() ?? [];
        }

        var exam = await examsDbContext.ReadExams
            .Include(x => x.Sections)
            .ThenInclude(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == attempt.SourceId, cancellationToken);
        if (exam is null)
        {
            return [];
        }

        var answeredQuestionIds = attempt.AttemptAnswers
            .Select(x => x.QuestionId)
            .ToHashSet();
        var result = new List<AttemptQuestionSelection>();
        var order = 1;

        foreach (var section in exam.Sections)
        {
            var selectedIds = section.Questions
                .Select(x => x.QuestionId)
                .OrderByDescending(answeredQuestionIds.Contains)
                .ThenBy(x => x)
                .Take(section.QuestionsToSelect);

            result.AddRange(selectedIds.Select(
                questionId => new AttemptQuestionSelection(questionId, order++, section.ScorePerQuestion)));
        }

        return result;
    }

    private static decimal NormalizedScore(
        TestPlatform.Core.Attempts.AttemptQuestion question,
        TestPlatform.Core.Attempts.AttemptAnswer? answer) =>
        answer is null
            ? 0
            : question.Question.AnswerDefinition.Evaluate(answer.ToEvaluationValue());

    private static bool IsCorrect(
        TestPlatform.Core.Attempts.AttemptQuestion question,
        TestPlatform.Core.Attempts.AttemptAnswer? answer) =>
        NormalizedScore(question, answer) >= 1m;

    private static decimal EarnedScore(
        TestPlatform.Core.Attempts.AttemptQuestion question,
        TestPlatform.Core.Attempts.AttemptAnswer? answer) =>
        NormalizedScore(question, answer) * question.Score;
}
