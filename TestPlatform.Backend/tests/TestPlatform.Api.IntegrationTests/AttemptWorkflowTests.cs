using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Api.IntegrationTests.Infrastructure;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Request;
using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Contracts.Tests.DTOs;
using TestPlatform.Contracts.Tags.DTOs;
using TestPlatform.Contracts.Users.DTOs;
using TestPlatform.Core.Exams;
using TestPlatform.Core.Exams.Enums;
using TestPlatform.Core.Questions;
using TestPlatform.Core.Questions.AnswerDefinition;
using TestPlatform.Infrastructure.Postgres;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class AttemptWorkflowTests(TestPlatformWebApplicationFactory factory)
    : IClassFixture<TestPlatformWebApplicationFactory>
{
    [Fact]
    public async Task ProtectedEndpoint_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = factory.CreateAnonymousClient();
        client.DefaultRequestHeaders.Add("X-Skip-Test-Authentication", "true");

        var response = await client.GetAsync("/attempts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_CanPublishTest_CompleteAttempt_AndReceiveResult()
    {
        using var client = factory.CreateClient();
        var identity = await client.GetFromJsonAsync<CurrentUserResponse>("/users/me");
        Assert.NotNull(identity);

        var question = Question.Create(
            QuestionContent.Create("Столица Франции", "Париж").Value,
            TextAnswerDefinition.Create("Париж").Value,
            identity.Id).Value;
        question.Publish();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
            dbContext.Questions.Add(question);
            await dbContext.SaveChangesAsync();
        }

        var createTestResponse = await client.PostAsJsonAsync(
            "/tests",
            new TestRequest("География", "Тренировочный тест"));
        Assert.Equal(HttpStatusCode.Created, createTestResponse.StatusCode);
        var testId = await createTestResponse.Content.ReadFromJsonAsync<Guid>();

        var addQuestionResponse = await client.PostAsJsonAsync(
            $"/tests/{testId}/questions",
            new AddTestQuestionRequest(question.Id));
        Assert.Equal(HttpStatusCode.NoContent, addQuestionResponse.StatusCode);

        var publishResponse = await client.PostAsync($"/tests/{testId}/publish", null);
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        var requestId = Guid.NewGuid();
        var startResponse = await client.PostAsJsonAsync(
            "/attempts",
            new StartRequest(AttemptTypeDto.Test, testId, requestId));
        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);
        var started = await startResponse.Content.ReadFromJsonAsync<StartAttemptResponse>();
        Assert.NotNull(started);
        Assert.Equal(1, started.AttemptNumber);

        var retryResponse = await client.PostAsJsonAsync(
            "/attempts",
            new StartRequest(AttemptTypeDto.Test, testId, requestId));
        var retried = await retryResponse.Content.ReadFromJsonAsync<StartAttemptResponse>();
        Assert.Equal(started.AttemptId, retried!.AttemptId);

        var answerResponse = await client.PutAsJsonAsync<AttemptAnswerRequest>(
            $"/attempts/{started.AttemptId}/answers",
            new TextAttemptAnswerRequest(question.Id, "париж"));
        Assert.Equal(HttpStatusCode.NoContent, answerResponse.StatusCode);

        var finishResponse = await client.PostAsync(
            $"/attempts/{started.AttemptId}/finish",
            null);
        Assert.Equal(HttpStatusCode.OK, finishResponse.StatusCode);
        var result = await finishResponse.Content.ReadFromJsonAsync<AttemptResultResponse>();
        var testResult = Assert.IsType<TestAttemptResultResponse>(result);
        Assert.Equal(1, testResult.CorrectAnswers);
        Assert.Equal(100, testResult.Percentage);
    }

    [Fact]
    public async Task Employee_CannotAccessContentManagementEndpoint()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Employee");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "integration-test-employee");
        client.DefaultRequestHeaders.Add("X-Test-Employee-Number", "TEST-EMPLOYEE");

        var response = await client.GetAsync("/questions");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task FileEndpoints_RequireAuthorizationForChanges_AndUseProblemDetails()
    {
        using var anonymousClient = factory.CreateAnonymousClient();
        anonymousClient.DefaultRequestHeaders.Add("X-Skip-Test-Authentication", "true");

        var deleteResponse = await anonymousClient.DeleteAsync($"/images/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);

        using var authenticatedClient = factory.CreateClient();
        var missingFileResponse = await authenticatedClient.GetAsync($"/images/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missingFileResponse.StatusCode);
        await AssertProblemCodeAsync(missingFileResponse, "file.not_found");
    }

    [Fact]
    public async Task ValidationFilter_ReturnsValidationProblem_AndExamRoutesRemainStable()
    {
        using var client = factory.CreateClient();

        var invalidTagResponse = await client.PostAsJsonAsync(
            "/tags",
            new TagRequest(string.Empty, string.Empty));
        Assert.Equal(HttpStatusCode.BadRequest, invalidTagResponse.StatusCode);
        using (var validationDocument = JsonDocument.Parse(
            await invalidTagResponse.Content.ReadAsStringAsync()))
        {
            Assert.Equal(
                "Request validation failed.",
                validationDocument.RootElement.GetProperty("title").GetString());
            Assert.True(validationDocument.RootElement.GetProperty("errors").TryGetProperty("Name", out _));
            Assert.True(validationDocument.RootElement.GetProperty("errors").TryGetProperty("Description", out _));
        }

        var createExamResponse = await client.PostAsJsonAsync(
            "/exams",
            new TestPlatform.Contracts.Exams.DTOs.ExamRequest(
                "Controller split",
                "The public route must remain unchanged"));
        Assert.Equal(HttpStatusCode.Created, createExamResponse.StatusCode);
        var examId = await createExamResponse.Content.ReadFromJsonAsync<Guid>();

        var getExamResponse = await client.GetAsync($"/exams/{examId}");
        Assert.Equal(HttpStatusCode.OK, getExamResponse.StatusCode);
    }

    [Fact]
    public async Task Teacher_CannotReadAnotherTeachersExam()
    {
        using var ownerClient = factory.CreateClient();
        var createResponse = await ownerClient.PostAsJsonAsync(
            "/exams",
            new TestPlatform.Contracts.Exams.DTOs.ExamRequest(
                "Private draft",
                "Owned by the first teacher"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var examId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        using var otherTeacherClient = factory.CreateClient();
        otherTeacherClient.DefaultRequestHeaders.Add("X-Test-Subject", "integration-test-other-teacher");
        otherTeacherClient.DefaultRequestHeaders.Add("X-Test-Employee-Number", "TEST-OTHER");

        var response = await otherTeacherClient.GetAsync($"/exams/{examId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertProblemCodeAsync(response, "exam.not_found");
    }

    [Fact]
    public async Task Exam_EnforcesAttemptsLimit_AndHidesReviewUntilClosed()
    {
        using var client = factory.CreateClient();
        var identity = await client.GetFromJsonAsync<CurrentUserResponse>("/users/me");
        Assert.NotNull(identity);

        var questions = Enumerable.Range(1, 3)
            .Select(index => CreatePublishedTextQuestion(identity.Id, index))
            .ToArray();
        var exam = CreatePublishedExam(identity.Id, questions);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
            dbContext.Questions.AddRange(questions);
            dbContext.Exams.Add(exam);
            await dbContext.SaveChangesAsync();
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
            var persistedExam = await dbContext.Exams.AsNoTracking().SingleAsync(item => item.Id == exam.Id);
            Assert.NotNull(persistedExam.PassingRule);
            Assert.Equal(60, persistedExam.PassingRule.MinPercent);
        }

        var editPublishedResponse = await client.PutAsJsonAsync(
            $"/exams/{exam.Id}/attempts-limit",
            new TestPlatform.Contracts.Exams.DTOs.UpdateExamAttemptsLimitRequest(2));
        Assert.Equal(HttpStatusCode.BadRequest, editPublishedResponse.StatusCode);
        await AssertProblemCodeAsync(editPublishedResponse, "exam.not_editable");

        var startResponse = await client.PostAsJsonAsync(
            "/attempts",
            new StartRequest(AttemptTypeDto.Exam, exam.Id, Guid.NewGuid()));
        Assert.True(
            startResponse.StatusCode == HttpStatusCode.Created,
            $"Expected 201 but received {(int)startResponse.StatusCode}: {await startResponse.Content.ReadAsStringAsync()}");
        var started = await startResponse.Content.ReadFromJsonAsync<StartAttemptResponse>();
        Assert.NotNull(started);

        var finishResponse = await client.PostAsync($"/attempts/{started.AttemptId}/finish", null);
        Assert.Equal(HttpStatusCode.OK, finishResponse.StatusCode);

        var reviewResponse = await client.GetAsync($"/attempts/{started.AttemptId}/result");
        Assert.Equal(HttpStatusCode.Conflict, reviewResponse.StatusCode);
        await AssertProblemCodeAsync(reviewResponse, "attempt.review_not_available");

        var secondStartResponse = await client.PostAsJsonAsync(
            "/attempts",
            new StartRequest(AttemptTypeDto.Exam, exam.Id, Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.Conflict, secondStartResponse.StatusCode);
        await AssertProblemCodeAsync(secondStartResponse, "exam.attempts_limit_reached");
    }

    [Fact]
    public async Task ExamAttempts_WithDifferentSelections_HaveStableQuestionCountAndScore()
    {
        using var client = factory.CreateClient();
        var identity = await client.GetFromJsonAsync<CurrentUserResponse>("/users/me");
        Assert.NotNull(identity);

        var questions = Enumerable.Range(10, 5)
            .Select(index => CreatePublishedTextQuestion(identity.Id, index))
            .ToArray();
        var exam = CreatePublishedExam(
            identity.Id,
            questions,
            questionsToSelect: 3,
            scorePerQuestion: 7,
            attemptsLimit: 2,
            reviewPolicy: ExamReviewPolicy.Immediately);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
            dbContext.Questions.AddRange(questions);
            dbContext.Exams.Add(exam);
            await dbContext.SaveChangesAsync();
        }

        var first = await StartExamAsync(client, exam.Id);
        var second = await StartExamAsync(client, exam.Id);

        Assert.Equal(3, first.SourceResponse.TotalQuestions);
        Assert.Equal(first.SourceResponse.TotalQuestions, second.SourceResponse.TotalQuestions);
        Assert.Equal(21, first.SourceResponse.Questions.Sum(question => question.Score));
        Assert.Equal(
            first.SourceResponse.Questions.Sum(question => question.Score),
            second.SourceResponse.Questions.Sum(question => question.Score));
    }

    private static Question CreatePublishedTextQuestion(Guid authorId, int index)
    {
        var question = Question.Create(
            QuestionContent.Create($"Question {index}", null).Value,
            TextAnswerDefinition.Create($"Answer {index}").Value,
            authorId).Value;
        question.Publish();
        return question;
    }

    private static Exam CreatePublishedExam(
        Guid authorId,
        IReadOnlyCollection<Question> questions,
        int? questionsToSelect = null,
        int scorePerQuestion = 1,
        int attemptsLimit = 1,
        ExamReviewPolicy reviewPolicy = ExamReviewPolicy.AfterExamClosed)
    {
        var exam = Exam.Create("Integration exam", "Attempts and review policy", authorId).Value;
        var sectionId = exam.AddSection(
            "Main",
            questionsToSelect ?? questions.Count,
            scorePerQuestion).Value;
        foreach (var question in questions)
        {
            Assert.True(exam.AddQuestionToSection(sectionId, question.Id).IsSuccess);
        }

        Assert.True(exam.ChangeAttemptsLimit(attemptsLimit).IsSuccess);
        Assert.True(exam.ChangeReviewPolicy(reviewPolicy).IsSuccess);
        if (reviewPolicy == ExamReviewPolicy.AfterExamClosed)
        {
            Assert.True(exam.ChangeSchedule(
                ExamSchedule.Create(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddHours(1)).Value).IsSuccess);
        }

        Assert.True(exam.ChangePassingRule(ExamPassingRule.Create(null, 60).Value).IsSuccess);
        Assert.True(exam.Publish().IsSuccess);
        return exam;
    }

    private static async Task<StartAttemptResponse> StartExamAsync(HttpClient client, Guid examId)
    {
        var response = await client.PostAsJsonAsync(
            "/attempts",
            new StartRequest(AttemptTypeDto.Exam, examId, Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<StartAttemptResponse>())!;
    }

    private static async Task AssertProblemCodeAsync(HttpResponseMessage response, string expectedCode)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(expectedCode, document.RootElement.GetProperty("code").GetString());
    }
}
