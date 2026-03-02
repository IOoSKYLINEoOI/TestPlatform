using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Questions;
using TestPlatform.Core.Questions;
using TestPlatform.Infrastructure.Postgres.Questions.Entities;

namespace TestPlatform.Infrastructure.Postgres.Questions;

public class QuestionsRepository : IQuestionsRepository
{
    private readonly TestPlatformDbContext _context;

    public QuestionsRepository(TestPlatformDbContext context) => _context = context;

    public async Task<Result<Guid>> AddAsync(Question question, CancellationToken cancellationToken)
    {
        var questionEntity = MapToEntity(question);

        await _context.Questions.AddAsync(questionEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(questionEntity.Id);
    }

    public async Task<Result> UpdateAsync(Question question, CancellationToken cancellationToken)
{
    var questionEntity = await _context.Questions
        .Include(q => q.AnswersOptions)
        .Include(q => q.Tags)
        .SingleOrDefaultAsync(q => q.Id == question.Id, cancellationToken);

    if (questionEntity == null)
        return Result.Failure($"Question with id {question.Id} not found");

    questionEntity.Text = question.Text;
    questionEntity.Points = question.Points;
    questionEntity.QuestionTypeId = (int)question.QuestionType;
    questionEntity.ImageUrl = question.ImageUrl;

    var incomingAnswers = question.AnswersOptions.ToDictionary(a => a.Id);
    var existingAnswers = questionEntity.AnswersOptions.ToDictionary(a => a.Id);

    var toRemove = questionEntity.AnswersOptions
        .Where(a => !incomingAnswers.ContainsKey(a.Id))
        .ToList();

    if (toRemove.Count > 0)
        _context.AnswerOptions.RemoveRange(toRemove);

    foreach (var incoming in incomingAnswers.Values)
    {
        if (existingAnswers.TryGetValue(incoming.Id, out var existing))
        {
            existing.Text = incoming.Text;
            existing.IsCorrect = incoming.IsCorrect;
            existing.ImageUrl = incoming.ImageUrl;
        }
        else
        {
            var newEntity = new AnswerOptionEntity
            {
                Id = incoming.Id,
                Text = incoming.Text,
                IsCorrect = incoming.IsCorrect,
                ImageUrl = incoming.ImageUrl,
                QuestionId = questionEntity.Id,
            };

            await _context.AnswerOptions.AddAsync(newEntity, cancellationToken);
        }
    }

    var incomingTagIds = question.TagIds.ToHashSet();
    var existingTagIds = questionEntity.Tags.Select(t => t.Id).ToHashSet();

    var tagsToRemove = questionEntity.Tags
        .Where(t => !incomingTagIds.Contains(t.Id))
        .ToList();

    foreach (var tag in tagsToRemove)
        questionEntity.Tags.Remove(tag);

    var tagIdsToAdd = incomingTagIds
        .Where(id => !existingTagIds.Contains(id))
        .ToList();

    if (tagIdsToAdd.Count > 0)
    {
        var tagsToAdd = await _context.Tags
            .Where(t => tagIdsToAdd.Contains(t.Id))
            .ToListAsync(cancellationToken);

        foreach (var tag in tagsToAdd)
            questionEntity.Tags.Add(tag);
    }

    await _context.SaveChangesAsync(cancellationToken);

    return Result.Success();
}

    public async Task<bool> ExistsAsync(Guid questionId, CancellationToken cancellationToken)
        => await _context.Questions.AnyAsync(q => q.Id == questionId, cancellationToken);

    public async Task<Result> DeleteAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var questionEntity = await FindQuestionAsync(questionId, cancellationToken);
        if(questionEntity is null)
            return Result.Failure($"Question with id {questionId} not found");

        _context.Questions.Remove(questionEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static QuestionEntity MapToEntity(Question question) =>
        new QuestionEntity
        {
            Id = question.Id,
            Text = question.Text,
            QuestionTypeId = (int)question.QuestionType,
            Points = question.Points,
            ImageUrl = question.ImageUrl,
            AnswersOptions = question.AnswersOptions
                .Select(x => new AnswerOptionEntity
                {
                    Id = x.Id,
                    Text = x.Text,
                    IsCorrect = x.IsCorrect,
                    ImageUrl = x.ImageUrl,
                }).ToList(),
        };

    private Task<QuestionEntity?> FindQuestionAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Questions
            .Include(q => q.AnswersOptions)
            .SingleOrDefaultAsync(q => q.Id == id, cancellationToken);
}