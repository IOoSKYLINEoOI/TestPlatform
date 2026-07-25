using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TestPlatform.Application.Users;
using TestPlatform.Infrastructure.Identity.Management;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class KeycloakIdentityAccountProvisionerTests
{
    [Fact]
    public async Task CreateAsync_RoleAssignmentFailure_DeletesCreatedKeycloakUser()
    {
        var handler = new SequenceHttpMessageHandler(
            Json(HttpStatusCode.OK, """{"access_token":"token"}"""),
            Json(HttpStatusCode.OK, "[]"),
            Json(HttpStatusCode.OK, "[]"),
            Response(
                HttpStatusCode.Created,
                location: "http://keycloak:8080/admin/realms/test-platform/users/user-123"),
            Json(
                HttpStatusCode.OK,
                """{"id":"role-1","name":"Employee","composite":false,"clientRole":false}"""),
            Response(HttpStatusCode.InternalServerError),
            Response(HttpStatusCode.NoContent));
        var provisioner = CreateProvisioner(handler);

        var result = await provisioner.CreateAsync(
            new IdentityAccountProvisioningRequest(
                "employee.login",
                "EMP-123",
                "Temporary-Password-123!",
                "Employee"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityAccountErrors.ProvisioningFailed, result.Error);
        Assert.Equal(HttpMethod.Delete, handler.Requests.Last().Method);
        Assert.EndsWith(
            "/users/user-123",
            handler.Requests.Last().RequestUri?.AbsolutePath,
            StringComparison.Ordinal);
    }

    private static KeycloakIdentityAccountProvisioner CreateProvisioner(
        HttpMessageHandler handler)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://keycloak:8080/"),
        };
        return new KeycloakIdentityAccountProvisioner(
            client,
            Options.Create(new KeycloakManagementOptions
            {
                BaseUrl = "http://keycloak:8080",
                Realm = "test-platform",
                ClientId = "test-platform-admin",
                ClientSecret = "secret",
            }),
            NullLogger<KeycloakIdentityAccountProvisioner>.Instance);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string json)
        => new(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    private static HttpResponseMessage Response(
        HttpStatusCode status,
        string? location = null)
    {
        var response = new HttpResponseMessage(status);
        if (location is not null)
        {
            response.Headers.Location = new Uri(location);
        }

        return response;
    }

    private sealed class SequenceHttpMessageHandler(
        params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(new HttpRequestMessage(request.Method, request.RequestUri));
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
