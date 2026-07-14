using CSharpFunctionalExtensions;

namespace TestPlatform.Application.Abstractions;

public interface IAccessService<TEntity>
{
    Task<Result<TEntity>> GetForModifyAsync(Guid id, CancellationToken ct);
}