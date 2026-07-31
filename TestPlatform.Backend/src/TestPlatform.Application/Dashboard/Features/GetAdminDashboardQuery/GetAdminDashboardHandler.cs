using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Tests;
using TestPlatform.Contracts.Dashboard.DTOs;
using TestPlatform.Core.Attempts.Enums;
using TestPlatform.Core.Exams.Enums;
using TestPlatform.Core.Tests.Enums;

namespace TestPlatform.Application.Dashboard.Features.GetAdminDashboardQuery;

public sealed record GetAdminDashboardQuery : IQuery;

public sealed class GetAdminDashboardHandler(
    IAttemptsReadDbContext attempts,
    IExamsReadDbContext exams,
    ITestsReadDbContext tests)
    : IQueryHandler<GetAdminDashboardQuery, AdminDashboardResponse>
{
    public async Task<Result<AdminDashboardResponse>> Handle(
        GetAdminDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var activeExams = await exams.ReadExams.CountAsync(
            exam => exam.Status == ExamStatus.Published,
            cancellationToken);
        var publishedTests = await tests.ReadTests.CountAsync(
            test => test.Status == TestStatus.Published,
            cancellationToken);
        var totalAttempts = await attempts.ReadAttempts.CountAsync(cancellationToken);
        var unfinishedAttempts = await attempts.ReadAttempts.CountAsync(
            attempt => attempt.Status == AttemptStatus.STARTED ||
                       attempt.Status == AttemptStatus.NOT_STARTED,
            cancellationToken);
        var finishedAttempts = await attempts.ReadAttempts.CountAsync(
            attempt => attempt.Status == AttemptStatus.FINISHED,
            cancellationToken);
        var finishedExamResults = attempts.ReadAttempts.Where(
            attempt => attempt.Status == AttemptStatus.FINISHED &&
                       attempt.Type == AttemptType.Exam &&
                       attempt.AttemptResult != null);
        var finishedExams = await finishedExamResults.CountAsync(cancellationToken);
        var passedExams = await finishedExamResults.CountAsync(
            attempt => attempt.AttemptResult!.Passed == true,
            cancellationToken);
        var passRate = finishedExams == 0 ? 0 : Math.Round((double)passedExams / finishedExams * 100, 1);

        return Result.Success(new AdminDashboardResponse(
            activeExams,
            publishedTests,
            totalAttempts,
            unfinishedAttempts,
            finishedAttempts,
            passRate));
    }
}
