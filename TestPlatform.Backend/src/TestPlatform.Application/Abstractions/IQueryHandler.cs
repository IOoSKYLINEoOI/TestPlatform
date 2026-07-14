using CSharpFunctionalExtensions;
using TestPlatform.Contracts.Attempts.DTOs;

namespace TestPlatform.Application.Abstractions;

public interface IQuery;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}