using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Tags;
using TestPlatform.Core.Tags;
using TestPlatform.Infrastructure.Postgres.Tags.Entities;

namespace TestPlatform.Infrastructure.Postgres.Tags;

public class TagsRepository : ITagsRepository
{
    private readonly TestPlatformDbContext _context;

    public TagsRepository(TestPlatformDbContext context) => _context = context;

    public async Task<Result<Guid>> AddAsync(Tag tag, CancellationToken cancellationToken)
    {
        var tagEntity = MapToEntity(tag);

        await _context.Tags.AddAsync(tagEntity,  cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(tagEntity.Id);
    }

    public async Task<Result> UpdateAsync(Guid id, string name, string description, CancellationToken cancellationToken)
    {
        var tagEntity = await FindTagAsync(id, cancellationToken);
        if (tagEntity is null)
            return Result.Failure($"Tag with id {id} not found");

        tagEntity.Name = name;
        tagEntity.Description = description;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<bool> ExistsAsync(Guid tagId, CancellationToken cancellationToken)
        => await _context.Tags.AnyAsync(q => q.Id == tagId, cancellationToken);

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tagEntity = await FindTagAsync(id, cancellationToken);
        if (tagEntity is null)
            return Result.Failure($"Tag with id {id} not found");

        _context.Tags.Remove(tagEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static TagEntity MapToEntity(Tag tag) => new TagEntity()
    {
        Id = tag.Id,
        Name = tag.Name,
        Description = tag.Description,
    };

    private Task<TagEntity?> FindTagAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Tags.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}