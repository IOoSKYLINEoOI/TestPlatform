using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Tests;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Tags.DTOs;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Infrastructure.Postgres.Tests;

public class TestsReadRepository : ITestsReadRepository
{
    private readonly TestPlatformDbContext _context;

    public TestsReadRepository(TestPlatformDbContext context) => _context = context;

    public async Task<TestFullResponse?> ReadTestByIdAsync(
        Guid? id,
        bool includeCorrectAnswer,
        CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.TimeLimitSeconds,
                t.Description,
                t.AuthorId,
                t.CoverImageName,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (test == null)
            return null;

        var questions = await _context.Questions
            .AsNoTracking()
            .Where(q => q.Tests.Any(t => t.Id == id))
            .Select(q => new QuestionResponse(
                q.Id,
                q.Text,
                q.QuestionTypeId,
                q.Points,
                q.ImageName,
                q.Tags.Select(tag => new TagResponse(
                    tag.Id,
                    tag.Name,
                    tag.Description)).ToList(),
                q.AnswersOptions.Select(a => new AnswerOptionResponse(
                    a.Id,
                    a.Text,
                    includeCorrectAnswer ? a.IsCorrect : null,
                    a.ImageName)).ToList()
            ))
            .ToListAsync(cancellationToken);

        var tags = questions
            .SelectMany(q => q.Tags ?? [])
            .DistinctBy(t => t.Id)
            .ToList();

        return new TestFullResponse(
            test.Id,
            test.Name,
            test.TimeLimitSeconds,
            test.Description,
            test.AuthorId,
            questions.Count,
            test.CoverImageName,
            tags,
            questions);
    }


    public async Task<List<TestResponse>> ReadAllTestAsync(CancellationToken cancellationToken) 
        => await _context.Tests
            .AsNoTracking()
            .Select(z => new TestResponse(
                z.Id,
                z.Name,
                z.TimeLimitSeconds,
                z.Description,
                z.AuthorId,
                z.Questions.Count,
                z.Questions
                    .SelectMany(q => q.Tags)
                    .Select(t => t.Id)
                    .Distinct()
                    .ToList()))
            .ToListAsync(cancellationToken);
}