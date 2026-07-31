using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;
using TestPlatform.Core.Exams;
using TestPlatform.Core.Exams.Enums;
using TestPlatform.Core.Questions;
using TestPlatform.Core.Questions.AnswerDefinition;
using TestPlatform.Core.Questions.AnswerDefinition.Abstractions;
using TestPlatform.Core.Questions.Enums;
using TestPlatform.Core.Tests.Enums;
using TestPlatform.Core.Users;
using TestAggregate = TestPlatform.Core.Tests.Test;

namespace TestPlatform.Infrastructure.Postgres.Seeding;

public sealed class DevelopmentDataSeeder(
    TestPlatformDbContext dbContext,
    ILogger<DevelopmentDataSeeder> logger)
{
    public const string SeedUserKeycloakId = "seed:test-platform";

    private const int TargetUsers = 20;
    private const int TargetTags = 12;
    private const int TargetQuestions = 150;
    private const int TargetTests = 20;
    private const int TargetExams = 8;
    private const int TargetAttempts = 240;

    private static readonly (string Name, string Description)[] TagDefinitions =
    [
        ("Fundamentals", "Basic knowledge used by demo assessments."),
        ("Mathematics", "Arithmetic and numerical questions."),
        ("Software Development", "Programming and software engineering questions."),
        ("Databases", "Relational databases, SQL, and persistence."),
        ("HTTP", "HTTP semantics, methods, and status codes."),
        ("Security", "Authentication, authorization, and secure development."),
        ("Docker", "Containers, images, and local infrastructure."),
        ("C#", "The C# language and .NET runtime."),
        ("Architecture", "Application boundaries and architectural patterns."),
        ("Testing", "Automated tests and quality practices."),
        ("DevOps", "Delivery, monitoring, and operations."),
        ("Algorithms", "Basic algorithms and data structures."),
    ];

    public async Task<DevelopmentSeedResult> SeedAsync(
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var createdUsers = await EnsureUsersAsync(cancellationToken);
        var tags = await EnsureTagsAsync(cancellationToken);
        var author = await dbContext.Users.SingleAsync(
            user => user.KeycloakId == SeedUserKeycloakId,
            cancellationToken);
        var questionsResult = await EnsureQuestionsAsync(author.Id, tags.All, cancellationToken);
        var publishedQuestions = questionsResult.All
            .Where(question => question.Status == QuestionStatus.Published)
            .ToArray();
        var testsResult = await EnsureTestsAsync(author.Id, publishedQuestions, cancellationToken);
        var examsResult = await EnsureExamsAsync(author.Id, publishedQuestions, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var createdAttempts = await EnsureAttemptsAsync(
            testsResult.All.Where(test => test.Status == TestStatus.Published).ToArray(),
            examsResult.All.Where(exam => exam.Status == ExamStatus.Published).ToArray(),
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var result = new DevelopmentSeedResult(
            Created: createdUsers + tags.Created + questionsResult.Created
                     + testsResult.Created + examsResult.Created + createdAttempts > 0,
            UsersCreated: createdUsers,
            TagsCreated: tags.Created,
            QuestionsCreated: questionsResult.Created,
            TestsCreated: testsResult.Created,
            ExamsCreated: examsResult.Created,
            AttemptsCreated: createdAttempts);

        logger.LogInformation(
            "Development seed top-up completed: +{Users} users, +{Tags} tags, +{Questions} questions, +{Tests} tests, +{Exams} exams, +{Attempts} attempts.",
            result.UsersCreated,
            result.TagsCreated,
            result.QuestionsCreated,
            result.TestsCreated,
            result.ExamsCreated,
            result.AttemptsCreated);
        return result;
    }

    private async Task<int> EnsureUsersAsync(CancellationToken cancellationToken)
    {
        var demoUsers = await dbContext.Users
            .Where(user => user.KeycloakId == SeedUserKeycloakId
                           || user.KeycloakId.StartsWith("seed:employee:"))
            .ToListAsync(cancellationToken);
        var created = 0;

        if (demoUsers.All(user => user.KeycloakId != SeedUserKeycloakId))
        {
            var author = User.Create(SeedUserKeycloakId, "DEMO-TEACHER").Value;
            dbContext.Users.Add(author);
            demoUsers.Add(author);
            created++;
        }

        for (var index = demoUsers.Count; index < TargetUsers; index++)
        {
            var ordinal = index;
            var user = User.Create(
                $"seed:employee:{ordinal:000}",
                $"DEMO-{ordinal:000}").Value;
            dbContext.Users.Add(user);
            demoUsers.Add(user);
            created++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return created;
    }

    private async Task<SeedCollection<Tag>> EnsureTagsAsync(
        CancellationToken cancellationToken)
    {
        var tags = await dbContext.Tags
            .OrderBy(tag => tag.Name)
            .ToListAsync(cancellationToken);
        var existingNames = tags
            .Select(tag => tag.NormalizedName)
            .ToHashSet(StringComparer.Ordinal);
        var created = 0;

        foreach (var definition in TagDefinitions.Take(TargetTags))
        {
            var normalized = definition.Name.ToUpperInvariant();
            if (existingNames.Contains(normalized))
            {
                continue;
            }

            var tag = Tag.Create(definition.Name, definition.Description).Value;
            dbContext.Tags.Add(tag);
            tags.Add(tag);
            existingNames.Add(normalized);
            created++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new SeedCollection<Tag>(tags, created);
    }

    private async Task<SeedCollection<Question>> EnsureQuestionsAsync(
        Guid authorId,
        IReadOnlyList<Tag> tags,
        CancellationToken cancellationToken)
    {
        var questions = await dbContext.Questions
            .Where(question => question.CreatedByUserId == authorId)
            .OrderBy(question => question.CreatedAt)
            .ToListAsync(cancellationToken);
        var created = 0;

        for (var index = questions.Count + 1; index <= TargetQuestions; index++)
        {
            var question = CreateQuestion(index, authorId);
            question.ReplaceTags(
            [
                tags[(index - 1) % tags.Count],
                tags[(index + 2) % tags.Count],
            ]);

            if (index % 10 != 0)
            {
                question.Publish();
            }

            dbContext.Questions.Add(question);
            questions.Add(question);
            created++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new SeedCollection<Question>(questions, created);
    }

    private async Task<SeedCollection<TestAggregate>> EnsureTestsAsync(
        Guid authorId,
        IReadOnlyList<Question> publishedQuestions,
        CancellationToken cancellationToken)
    {
        var tests = await dbContext.Tests
            .Where(test => test.AuthorId == authorId)
            .Include(test => test.Questions)
            .OrderBy(test => test.CreatedAt)
            .ToListAsync(cancellationToken);
        var created = 0;

        for (var index = tests.Count + 1; index <= TargetTests; index++)
        {
            var test = TestAggregate.Create(
                $"Demo Practice Test {index:00}",
                $"Generated practice set {index:00} for pagination and workflow testing.",
                authorId).Value;
            test.ChangeTimeLimit(600 + (index % 5 * 300));

            foreach (var question in TakeCircular(publishedQuestions, index * 7, 15))
            {
                test.AddQuestion(question.Id);
            }

            if (index <= 16)
            {
                test.Publish();
            }

            dbContext.Tests.Add(test);
            tests.Add(test);
            created++;
        }

        return new SeedCollection<TestAggregate>(tests, created);
    }

    private async Task<SeedCollection<Exam>> EnsureExamsAsync(
        Guid authorId,
        IReadOnlyList<Question> publishedQuestions,
        CancellationToken cancellationToken)
    {
        var exams = await dbContext.Exams
            .Where(exam => exam.AuthorId == authorId)
            .AsSplitQuery()
            .Include(exam => exam.Sections)
            .ThenInclude(section => section.Questions)
            .OrderBy(exam => exam.CreatedAt)
            .ToListAsync(cancellationToken);
        var created = 0;

        for (var index = exams.Count + 1; index <= TargetExams; index++)
        {
            var exam = Exam.Create(
                $"Demo Certification Exam {index:00}",
                $"Generated exam {index:00} with a stable maximum score and randomized selection pool.",
                authorId).Value;
            var sectionId = exam.AddSection(
                "General pool",
                questionsToSelect: 5,
                scorePerQuestion: 10).Value;

            foreach (var question in TakeCircular(publishedQuestions, index * 13, 30))
            {
                exam.AddQuestionToSection(sectionId, question.Id);
            }

            exam.ChangeAttemptsLimit(2 + index % 3);
            exam.ChangeTimeLimit(1_200 + index * 60);
            exam.ChangeReviewPolicy(ExamReviewPolicy.Immediately);
            exam.ChangePassingRule(ExamPassingRule.Create(null, 70).Value);
            if (index <= 7)
            {
                exam.Publish();
            }

            dbContext.Exams.Add(exam);
            exams.Add(exam);
            created++;
        }

        return new SeedCollection<Exam>(exams, created);
    }

    private async Task<int> EnsureAttemptsAsync(
        IReadOnlyList<TestAggregate> tests,
        IReadOnlyList<Exam> exams,
        CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .Where(user => user.KeycloakId.StartsWith("seed:employee:"))
            .OrderBy(user => user.EmployeeNumber)
            .ToListAsync(cancellationToken);
        var userIds = users.Select(user => user.Id).ToArray();
        if (await dbContext.Attempts.AnyAsync(
                attempt => userIds.Contains(attempt.UserId),
                cancellationToken))
        {
            return 0;
        }

        var sources = tests
            .Select(test => new AttemptSeedSource(
                AttemptType.Test,
                test.Id,
                test.TimeLimitSeconds,
                null,
                test.Questions
                    .OrderBy(question => question.Order)
                    .Select(question => new AttemptQuestionSelection(
                        question.QuestionId,
                        question.Order,
                        1))
                    .ToArray()))
            .Concat(exams.Select(exam => new AttemptSeedSource(
                AttemptType.Exam,
                exam.Id,
                exam.TimeLimitSeconds,
                exam.PassingRule?.MinPercent,
                exam.Sections
                    .SelectMany(section => section.Questions
                        .Take(section.QuestionsToSelect)
                        .Select((question, index) => new AttemptQuestionSelection(
                            question.QuestionId,
                            index + 1,
                            section.ScorePerQuestion)))
                    .ToArray())))
            .ToArray();

        for (var index = 0; index < TargetAttempts; index++)
        {
            var user = users[index % users.Count];
            var source = sources[(index / users.Count) % sources.Length];
            var attempt = Attempt.Create(
                user.Id,
                source.Type,
                source.Id,
                source.Selections,
                source.TimeLimitSeconds,
                minPassingPercent: source.MinPassingPercent,
                requestId: Guid.NewGuid()).Value;
            attempt.AssignAttemptNumber(1);
            ApplyAttemptStatus(attempt, index);
            dbContext.Attempts.Add(attempt);
        }

        return TargetAttempts;
    }

    private static Question CreateQuestion(int index, Guid authorId)
    {
        QuestionAnswerDefinition answer = (index % 4) switch
        {
            0 => TextAnswerDefinition.Create($"answer-{index:000}").Value,
            1 => NumberAnswerDefinition.Create(index * 3).Value,
            2 => CreateSingleChoice(index),
            _ => CreateMultipleChoice(index),
        };
        return Question.Create(
            QuestionContent.Create(
                $"Demo question {index:000}: choose or enter the expected value.",
                $"Generated explanation for demo question {index:000}.").Value,
            answer,
            authorId).Value;
    }

    private static ChoiceAnswerDefinition CreateSingleChoice(int index)
        => ChoiceAnswerDefinition.Create(
            ChoiceMode.Single,
            EvaluationMode.Strict,
            new[]
            {
                AnswerOption.Create($"Correct option {index}", true, null).Value,
                AnswerOption.Create($"Distractor A {index}", false, null).Value,
                AnswerOption.Create($"Distractor B {index}", false, null).Value,
                AnswerOption.Create($"Distractor C {index}", false, null).Value,
            }).Value;

    private static ChoiceAnswerDefinition CreateMultipleChoice(int index)
        => ChoiceAnswerDefinition.Create(
            ChoiceMode.Multiple,
            EvaluationMode.Partial,
            new[]
            {
                AnswerOption.Create($"Correct option A {index}", true, null).Value,
                AnswerOption.Create($"Correct option B {index}", true, null).Value,
                AnswerOption.Create($"Distractor A {index}", false, null).Value,
                AnswerOption.Create($"Distractor B {index}", false, null).Value,
            }).Value;

    private static IReadOnlyList<T> TakeCircular<T>(
        IReadOnlyList<T> source,
        int start,
        int count)
    {
        return Enumerable.Range(0, count)
            .Select(offset => source[(start + offset) % source.Count])
            .ToArray();
    }

    private static void ApplyAttemptStatus(Attempt attempt, int index)
    {
        if (index % 12 == 0)
        {
            return;
        }

        attempt.Start();
        switch (index % 6)
        {
            case 0:
            case 1:
            case 2:
                attempt.Finish([]);
                break;
            case 3:
                attempt.Abandon();
                break;
            case 4:
                attempt.Expire();
                break;
            case 5:
                break;
        }
    }

    private sealed record SeedCollection<T>(IReadOnlyList<T> All, int Created);

    private sealed record AttemptSeedSource(
        AttemptType Type,
        Guid Id,
        int? TimeLimitSeconds,
        double? MinPassingPercent,
        IReadOnlyList<AttemptQuestionSelection> Selections);
}

public sealed record DevelopmentSeedResult(
    bool Created,
    int UsersCreated,
    int TagsCreated,
    int QuestionsCreated,
    int TestsCreated,
    int ExamsCreated,
    int AttemptsCreated);
