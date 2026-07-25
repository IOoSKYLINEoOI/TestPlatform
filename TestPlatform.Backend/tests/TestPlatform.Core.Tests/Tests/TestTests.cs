using TestPlatform.Core.Tests.Enums;
using Xunit;

namespace TestPlatform.Core.Tests.Tests;

public class TestTests
{
    [Fact]
    public void EmptyTest_CannotBePublished()
    {
        var test = CreateTest();

        var result = test.Publish();

        Assert.True(result.IsFailure);
        Assert.Equal(TestStatus.Draft, test.Status);
    }

    [Fact]
    public void PublishedTest_CannotBeEdited()
    {
        var test = CreateTest();
        test.AddQuestion(Guid.NewGuid());
        test.Publish();

        var result = test.ChangeTitle("Updated title");

        Assert.True(result.IsFailure);
        Assert.Equal("test.not_editable", result.Error);
    }

    [Fact]
    public void PublishedTest_CanBeArchived()
    {
        var test = CreateTest();
        test.AddQuestion(Guid.NewGuid());
        test.Publish();

        var result = test.Archive();

        Assert.True(result.IsSuccess);
        Assert.Equal(TestStatus.Archived, test.Status);
    }

    private static Test CreateTest() =>
        Test.Create("Training test", "Description", Guid.NewGuid()).Value;
}
