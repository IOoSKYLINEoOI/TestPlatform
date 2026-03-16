using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Questions;
using TestPlatform.Application.Questions.Validators;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Infrastructure.Postgres.Questions;

public class QuestionsReadRepository : IQuestionsReadRepository
{
    private readonly TestPlatformDbContext _context;

    public QuestionsReadRepository(TestPlatformDbContext context) => _context = context;

    public async Task<QuestionResponse?> ReadQuestionByIdAsync(Guid id, bool includeCorrectAnswer, CancellationToken cancellationToken)
        => await _context.Questions
       .AsNoTracking()
       .Include(q => q.AnswersOptions)
       .Where(q => q.Id == id)
       .Select(q => new QuestionResponse(
           q.Id,
           q.Text,
           q.QuestionTypeId,
           q.Points,
           q.ImageName,
           q.Tags.Select(z => new TagResponse(
               z.Id,
               z.Name,
               z.Description))
               .ToList(),
           q.AnswersOptions.Select(a => new AnswerOptionResponse(
               a.Id,
               a.Text,
               includeCorrectAnswer ? a.IsCorrect : null,
               a.ImageName))
               .ToList()))
       .SingleOrDefaultAsync(cancellationToken);

    public async Task<List<QuestionResponse>> ReadAllQuestionsByTagsAsync(IReadOnlyList<Guid> tagIds, bool includeCorrectAnswer, CancellationToken cancellationToken)
        => await _context.Questions
            .AsNoTracking()
            .Where(q => q.Tags.Any(t => tagIds.Contains(t.Id)))
            .Select(q => new QuestionResponse(
                q.Id,
                q.Text,
                q.QuestionTypeId,
                q.Points,
                q.ImageName,
                q.Tags
                    .Select(t => new TagResponse(
                        t.Id,
                        t.Name,
                        t.Description))
                    .ToList(),
                q.AnswersOptions
                    .Select(a => new AnswerOptionResponse(
                        a.Id,
                        a.Text,
                        includeCorrectAnswer ? a.IsCorrect : null,
                        a.ImageName))
                    .ToList()))
            .ToListAsync(cancellationToken);
}