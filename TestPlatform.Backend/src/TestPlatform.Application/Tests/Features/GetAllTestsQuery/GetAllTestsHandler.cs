using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Application.Tests.Features.GetAllTestsQuery;

public record GetAllTestsQuery() : IQuery;

public class GetAllTestsHandler : IQueryHandler<List<TestResponse>, GetAllTestsQuery>
{
    private readonly ITestsReadRepository _testsReadRepository;
    private readonly ILogger<GetAllTestsHandler> _logger;

    public GetAllTestsHandler(ITestsReadRepository testsReadRepository, ILogger<GetAllTestsHandler> logger)
    {
        _testsReadRepository = testsReadRepository;
        _logger = logger;
    }

    public async Task<List<TestResponse>?> Handle(GetAllTestsQuery request, CancellationToken cancellationToken)
    {
        var tests = await _testsReadRepository.ReadAllTestAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} tags", tests.Count);

        return tests;
    }
}