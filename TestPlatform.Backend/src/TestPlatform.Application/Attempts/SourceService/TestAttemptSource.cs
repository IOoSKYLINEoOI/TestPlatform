using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Application.Attempts.SourceService;

public class TestAttemptSource : IAttemptSource
{
    private readonly TestFullResponse _test;

    public TestAttemptSource(TestFullResponse test)
    {
        _test = test;
    }

    public Guid Id => _test.Id;

    public int? TimeLimitSeconds => _test.TimeLimitSeconds;

    public IReadOnlyCollection<QuestionResponse> Questions => _test.Questions;
}