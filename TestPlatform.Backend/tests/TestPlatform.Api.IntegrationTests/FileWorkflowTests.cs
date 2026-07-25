using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Api.Files;
using TestPlatform.Api.IntegrationTests.Infrastructure;
using TestPlatform.Application.Files;
using TestPlatform.Core.Files;
using TestPlatform.Infrastructure.Postgres;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class FileWorkflowTests(TestPlatformWebApplicationFactory factory)
    : IClassFixture<TestPlatformWebApplicationFactory>
{
    [Fact]
    public async Task Upload_Read_AndAdminDelete_KeepDatabaseAndStorageConsistent()
    {
        var sourceBytes = new byte[] { 1, 2, 3, 4, 5, 6 };
        using var ownerClient = factory.CreateClient();
        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(sourceBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", "avatar.png");

        var uploadResponse = await ownerClient.PostAsync("/images", form);

        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<UploadImageResponse>();
        Assert.NotNull(uploaded);
        Assert.EndsWith($"/images/{uploaded.FileId}", uploaded.Url, StringComparison.Ordinal);
        Assert.EndsWith(
            $"/images/{uploaded.FileId}",
            uploadResponse.Headers.Location?.OriginalString,
            StringComparison.Ordinal);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
            var fileAsset = await dbContext.FileAssets
                .AsNoTracking()
                .SingleAsync(file => file.Id == uploaded.FileId);
            Assert.Equal("image/webp", fileAsset.ContentType);
            Assert.EndsWith(".webp", fileAsset.FileName, StringComparison.Ordinal);
            Assert.Equal(sourceBytes.Length, fileAsset.SizeBytes);
            Assert.Equal(FileAssetStatus.Temporary, fileAsset.Status);
        }

        var readResponse = await ownerClient.GetAsync($"/images/{uploaded.FileId}");
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
        Assert.Equal("image/webp", readResponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal(sourceBytes, await readResponse.Content.ReadAsByteArrayAsync());

        using var otherTeacherClient = factory.CreateClient();
        otherTeacherClient.DefaultRequestHeaders.Add("X-Test-Subject", "file-other-teacher");
        otherTeacherClient.DefaultRequestHeaders.Add("X-Test-Employee-Number", "FILE-OTHER");
        var forbiddenResponse = await otherTeacherClient.DeleteAsync($"/images/{uploaded.FileId}");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        await AssertProblemCodeAsync(forbiddenResponse, "file.forbidden");

        using var adminClient = factory.CreateClient();
        adminClient.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        adminClient.DefaultRequestHeaders.Add("X-Test-Subject", "file-admin");
        adminClient.DefaultRequestHeaders.Add("X-Test-Employee-Number", "FILE-ADMIN");
        var deleteResponse = await adminClient.DeleteAsync($"/images/{uploaded.FileId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var storage = Assert.IsType<InMemoryObjectStorage>(
            factory.Services.GetRequiredService<IObjectStorage>());
        var deleteCalls = storage.DeleteCallCount;
        var repeatedDeleteResponse = await adminClient.DeleteAsync($"/images/{uploaded.FileId}");
        Assert.Equal(HttpStatusCode.NoContent, repeatedDeleteResponse.StatusCode);
        Assert.Equal(deleteCalls, storage.DeleteCallCount);

        var deletedReadResponse = await ownerClient.GetAsync($"/images/{uploaded.FileId}");
        Assert.Equal(HttpStatusCode.NotFound, deletedReadResponse.StatusCode);
        await AssertProblemCodeAsync(deletedReadResponse, "file.not_found");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
            var fileAsset = await dbContext.FileAssets
                .AsNoTracking()
                .SingleAsync(file => file.Id == uploaded.FileId);
            Assert.Equal(FileAssetStatus.Deleted, fileAsset.Status);
            Assert.NotNull(fileAsset.DeletedAt);
        }
    }

    [Fact]
    public async Task FailedStorageDelete_LeavesFilePendingForBackgroundRetry()
    {
        using var ownerClient = factory.CreateClient();
        var uploaded = await UploadAsync(ownerClient, new byte[] { 10, 20, 30 });
        var storage = Assert.IsType<InMemoryObjectStorage>(
            factory.Services.GetRequiredService<IObjectStorage>());
        storage.FailNextDelete();

        var failedDeleteResponse = await ownerClient.DeleteAsync($"/images/{uploaded.FileId}");

        Assert.Equal(HttpStatusCode.BadRequest, failedDeleteResponse.StatusCode);
        await AssertProblemCodeAsync(failedDeleteResponse, "file.delete_error");

        var readResponse = await ownerClient.GetAsync($"/images/{uploaded.FileId}");
        Assert.Equal(HttpStatusCode.NotFound, readResponse.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
        var fileAsset = await dbContext.FileAssets
            .AsNoTracking()
            .SingleAsync(file => file.Id == uploaded.FileId);
        Assert.Equal(FileAssetStatus.DeletionPending, fileAsset.Status);
        Assert.Null(fileAsset.DeletedAt);

        var cleanup = scope.ServiceProvider.GetRequiredService<ITemporaryFileCleanupService>();
        var cleanupResult = await cleanup.CleanupAsync(
            DateTime.UtcNow,
            batchSize: 10,
            CancellationToken.None);
        Assert.True(cleanupResult.Deleted >= 1);

        dbContext.ChangeTracker.Clear();
        var deletedFileAsset = await dbContext.FileAssets
            .AsNoTracking()
            .SingleAsync(file => file.Id == uploaded.FileId);
        Assert.Equal(FileAssetStatus.Deleted, deletedFileAsset.Status);
        Assert.NotNull(deletedFileAsset.DeletedAt);
    }

    [Fact]
    public async Task Cleanup_RemovesOnlyTemporaryFilesOlderThanCutoff()
    {
        using var ownerClient = factory.CreateClient();
        var uploaded = await UploadAsync(ownerClient, [7, 8, 9]);

        await using var scope = factory.Services.CreateAsyncScope();
        var cleanup = scope.ServiceProvider.GetRequiredService<ITemporaryFileCleanupService>();
        var result = await cleanup.CleanupAsync(
            DateTime.UtcNow.AddMinutes(1),
            batchSize: 10,
            CancellationToken.None);

        Assert.True(result.Found >= 1);
        Assert.Equal(result.Found, result.Deleted);
        Assert.Equal(0, result.Failed);
        var readResponse = await ownerClient.GetAsync($"/images/{uploaded.FileId}");
        Assert.Equal(HttpStatusCode.NotFound, readResponse.StatusCode);
    }

    private static async Task<UploadImageResponse> UploadAsync(
        HttpClient client,
        byte[] content)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", "image.png");
        var response = await client.PostAsync("/images", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<UploadImageResponse>())!;
    }

    private static async Task AssertProblemCodeAsync(
        HttpResponseMessage response,
        string expectedCode)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(expectedCode, document.RootElement.GetProperty("code").GetString());
    }
}
