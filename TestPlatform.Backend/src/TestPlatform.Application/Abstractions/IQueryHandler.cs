using CSharpFunctionalExtensions;
using TestPlatform.Contracts.Attempts.DTOs;

namespace TestPlatform.Application.Abstractions;

public interface IQuery;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}