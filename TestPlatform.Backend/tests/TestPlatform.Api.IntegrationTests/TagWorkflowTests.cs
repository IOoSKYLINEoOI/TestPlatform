using System.Net;
using System.Net.Http.Json;
using TestPlatform.Api.IntegrationTests.Infrastructure;
using TestPlatform.Contracts.Tags.DTOs;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class TagWorkflowTests(TestPlatformWebApplicationFactory factory)
    : IClassFixture<TestPlatformWebApplicationFactory>
{
    [Fact]
    public async Task GetTags_SearchesOrdersAndPaginates()
    {
        using var client = factory.CreateClient();
        var prefix = $"Page-{Guid.NewGuid():N}";
        await CreateTagAsync(client, $"{prefix}-C");
        await CreateTagAsync(client, $"{prefix}-A");
        await CreateTagAsync(client, $"{prefix}-B");

        var firstPage = await client.GetFromJsonAsync<TagPageResponse>(
            $"/tags?search={Uri.EscapeDataString(prefix)}&page=1&pageSize=2");
        var secondPage = await client.GetFromJsonAsync<TagPageResponse>(
            $"/tags?search={Uri.EscapeDataString(prefix)}&page=2&pageSize=2");

        Assert.NotNull(firstPage);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(2, firstPage.PageSize);
        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(
            [$"{prefix}-A", $"{prefix}-B"],
            firstPage.Items.Select(tag => tag.Name));

        Assert.NotNull(secondPage);
        Assert.Equal(2, secondPage.Page);
        Assert.Equal(3, secondPage.TotalCount);
        Assert.Single(secondPage.Items);
        Assert.Equal($"{prefix}-C", secondPage.Items[0].Name);
    }

    private static async Task CreateTagAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync(
            "/tags",
            new TagRequest(name, $"Description for {name}."));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
