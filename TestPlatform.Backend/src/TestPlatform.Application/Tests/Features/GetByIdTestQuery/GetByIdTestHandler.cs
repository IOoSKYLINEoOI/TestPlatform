using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Application.Tests.Features.GetByIdTestQuery;

public record GetByIdTestQuery(Guid Id, bool IncludeCorrectAnswer) : IQuery;

public class GetByIdTestHandler : IQueryHandler<TestFullResponse, GetByIdTestQuery>
{
    private readonly ITestsReadRepository _testsReadRepository;
    private readonly ILogger<GetByIdTestHandler> _logger;

    public GetByIdTestHandler(ITestsReadRepository testsReadRepository, ILogger<GetByIdTestHandler> logger)
    {
        _testsReadRepository = testsReadRepository;
        _logger = logger;
    }

    public async Task<Result<TestFullResponse>> Handle(GetByIdTestQuery query, CancellationToken cancellationToken)
    {
        var test =
            await _testsReadRepository.ReadTestByIdAsync(query.Id, query.IncludeCorrectAnswer, cancellationToken);

        if (test == null)
        {
            _logger.LogWarning("Test with id {Id} not found", query.Id);
            return Result.Failure<TestFullResponse>("Test not found");
        }

        _logger.LogInformation("Get Test with id {Id}", query.Id);
        return Result.Success(test);
    }
}