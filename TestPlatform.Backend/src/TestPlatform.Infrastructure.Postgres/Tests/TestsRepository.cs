using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Tests;
using TestPlatform.Core.Tests;
using TestPlatform.Infrastructure.Postgres.Tests.Entities;

namespace TestPlatform.Infrastructure.Postgres.Tests;

public class TestsRepository : ITestsRepository
{
    private readonly TestPlatformDbContext _context;

    public TestsRepository(TestPlatformDbContext context) => _context = context;

    public async Task<Result<Guid>> AddAsync(Test test, CancellationToken cancellationToken)
    {
        var questions = await _context.Questions
            .Where(q => test.QuestionsIds.Contains(q.Id))
            .ToListAsync(cancellationToken);

        var testEntity = new TestEntity()
        {
            Id = test.Id,
            Name = test.Name,
            Description = test.Description,
            AuthorId = test.AuthorId,
            CoverImageName = test.CoverImageName,
            TimeLimitSeconds = test.TimeLimitSeconds,
            Questions = questions,
        };

        await _context.Tests.AddAsync(testEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(testEntity.Id);
    }

    public async Task<Result> UpdateAsync(Test test, CancellationToken cancellationToken)
    {
        var testEntity = await _context.Tests
            .Include(t => t.Questions)
            .SingleOrDefaultAsync(t => t.Id == test.Id, cancellationToken);

        if (testEntity == null)
            return Result.Failure($"Test with id {test.Id} not found");

        testEntity.Name = test.Name;
        testEntity.Description = test.Description;
        testEntity.AuthorId = test.AuthorId;
        testEntity.CoverImageName = test.CoverImageName;
        testEntity.TimeLimitSeconds = test.TimeLimitSeconds;

        var incomingQuestionIds = test.QuestionsIds.ToHashSet();
        var existingQuestionIds = testEntity.Questions.Select(q => q.Id).ToHashSet();

        var questionsToRemove = testEntity.Questions
            .Where(q => !incomingQuestionIds.Contains(q.Id))
            .ToList();

        foreach (var q in questionsToRemove)
            testEntity.Questions.Remove(q);

        var questionIdsToAdd = incomingQuestionIds
            .Where(id => !existingQuestionIds.Contains(id))
            .ToList();

        if (questionIdsToAdd.Any())
        {
            var questionsToAdd = await _context.Questions
                .Where(q => questionIdsToAdd.Contains(q.Id))
                .ToListAsync(cancellationToken);

            foreach (var question in questionsToAdd)
                testEntity.Questions.Add(question);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid testId, CancellationToken cancellationToken)
    {
        var testEntity = await _context.Tests.FirstOrDefaultAsync(q => q.Id == testId, cancellationToken);
        if (testEntity == null)
            return Result.Failure<Guid>($"Test with id {testId} not found");

        _context.Tests.Remove(testEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}