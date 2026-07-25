using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TestPlatform.Application.Attempts;
using TestPlatform.Application.Files;
using TestPlatform.Application.Users;
using TestPlatform.Infrastructure.Postgres;

namespace TestPlatform.Api.IntegrationTests.Infrastructure;

public sealed class TestPlatformWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public HttpClient CreateAnonymousClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Seq:ServerUrl", "http://localhost:5341");
        builder.UseSetting(
            "ConnectionStrings:TestPlatformContextPostgreSQL",
            "Host=localhost;Database=test;Username=test;Password=test");
        builder.UseSetting("ImageStorage:Minio:AccessKey", "integration-test");
        builder.UseSetting("ImageStorage:Minio:SecretKey", "integration-test");
        builder.UseSetting("IdentityManagement:BaseUrl", "http://localhost:8080");
        builder.UseSetting("IdentityManagement:Realm", "test-platform");
        builder.UseSetting("IdentityManagement:ClientId", "integration-test");
        builder.UseSetting("IdentityManagement:ClientSecret", "integration-test");
        builder.ConfigureServices(services =>
        {
            _connection.Open();
            var efServices = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

            services.RemoveAll<DbContextOptions<TestPlatformDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<TestPlatformDbContext>>();
            services.AddDbContext<TestPlatformDbContext>(options =>
                options
                    .UseSqlite(_connection)
                    .UseInternalServiceProvider(efServices));

            services.RemoveAll<IAttemptStartStore>();
            services.AddScoped<IAttemptStartStore, TestAttemptStartStore>();
            services.RemoveAll<IObjectStorage>();
            services.RemoveAll<IImageProcessor>();
            services.RemoveAll<IIdentityAccountProvisioner>();
            services.AddSingleton<IObjectStorage, InMemoryObjectStorage>();
            services.AddSingleton<IImageProcessor, PassThroughImageProcessor>();
            services.AddSingleton<IIdentityAccountProvisioner, TestIdentityAccountProvisioner>();
            services.RemoveAll<IHostedService>();
            services.RemoveAll<IDataProtectionProvider>();
            services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultScheme = TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<TestPlatformDbContext>()
            .Database
            .EnsureCreated();
        return host;
    }
}
