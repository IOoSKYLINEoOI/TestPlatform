using TestPlatform.Contracts.Tests.Enums;
using TestPlatform.Core.Tests.Enums;

namespace TestPlatform.Application.Tests.Mappers;

public static class TestStatusMapping
{
    public static TestStatusDto ToDto(this TestStatus status) => status switch
    {
        TestStatus.Draft => TestStatusDto.Draft,
        TestStatus.Published => TestStatusDto.Published,
        TestStatus.Archived => TestStatusDto.Archived,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };
}
