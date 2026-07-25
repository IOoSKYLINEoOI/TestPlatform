using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using TestPlatform.Application;
using TestPlatform.Application.Users;
using TestPlatform.Api.Common.Validation;
using TestPlatform.Contracts.Authorization;
using TestPlatform.Infrastructure.Files.ImageProcessing;
using TestPlatform.Infrastructure.Files.ObjectStorage.Minio;
using TestPlatform.Infrastructure.Identity;
using TestPlatform.Infrastructure.Postgres;
using TestPlatform.Infrastructure.Postgres.Seeding;
using TestPlatform.Web.BackgroundServices;
using TestPlatform.Web.Errors;
using TestPlatform.Web.Health;
using TestPlatform.Web.OpenApi;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddUserSecrets<Program>();
    builder.Services.AddSerilog((_, loggerConfiguration) =>
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "TestPlatform")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .WriteTo.Console()
            .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"]
                ?? throw new InvalidOperationException("Seq:ServerUrl is not configured."));
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGenWithAuthSupport(builder.Configuration);

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(AuthorizationPolicies.ManageContent, policy => policy.RequireRole("Admin", "Teacher"));
        options.AddPolicy(AuthorizationPolicies.ManageTags, policy => policy.RequireRole("Admin", "Teacher"));
        options.AddPolicy(AuthorizationPolicies.ManageSystem, policy => policy.RequireRole("Admin"));
    });

    builder.Services.AddTestPlatformIdentity(
        builder.Configuration,
        builder.Environment.IsDevelopment());

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy
                .WithOrigins("http://localhost:5175")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    builder.Services
        .AddControllers(config =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            config.Filters.Add(new AuthorizeFilter(policy));
            config.Filters.Add<FluentValidationFilter>();
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

    builder.Services
        .AddTestPlatformPersistence(builder.Configuration)
        .AddTestPlatformApplication()
        .AddTestPlatformMinioObjectStorage(builder.Configuration)
        .AddTestPlatformImageProcessing(builder.Configuration);

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddOptions<AttemptExpirationOptions>()
        .Bind(builder.Configuration.GetSection(AttemptExpirationOptions.SectionName))
        .Validate(options => options.IntervalSeconds > 0, "Attempt expiration interval must be positive.")
        .ValidateOnStart();
    builder.Services.AddHostedService<AttemptExpirationWorker>();
    builder.Services.AddOptions<TemporaryFileCleanupOptions>()
        .Bind(builder.Configuration.GetSection(TemporaryFileCleanupOptions.SectionName))
        .Validate(options => options.RetentionHours > 0, "Temporary file retention must be positive.")
        .Validate(options => options.IntervalMinutes > 0, "Cleanup interval must be positive.")
        .Validate(options => options.BatchSize > 0, "Cleanup batch size must be positive.")
        .ValidateOnStart();
    builder.Services.AddHostedService<TemporaryFileCleanupWorker>();

    builder.Services.AddHealthChecks()
        .AddCheck(
            "self",
            () => HealthCheckResult.Healthy(),
            tags: ["live"])
        .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"])
        .AddCheck<MinioHealthCheck>("minio", tags: ["ready"]);

    builder.Services.AddExceptionHandler<ApiExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(5062));

    var app = builder.Build();

    if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
        Log.Information("Applying TestPlatform database migrations.");
        await dbContext.Database.MigrateAsync();
        Log.Information("TestPlatform database migrations applied successfully.");
        return;
    }

    if (args.Contains("--seed", StringComparer.OrdinalIgnoreCase))
    {
        if (!app.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Development seed data can only be created in the Development environment.");
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
        await dbContext.Database.MigrateAsync();
        var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
        return;
    }

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.OAuthClientId("public-client");
            c.OAuthUsePkce();
        });
    }

    app.UseStaticFiles();
    app.UseRouting();

    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = (context, _, exception) =>
        {
            if (exception is not null || context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                return LogEventLevel.Error;
            }

            return context.Response.StatusCode >= StatusCodes.Status400BadRequest
                ? LogEventLevel.Warning
                : LogEventLevel.Information;
        };

        options.EnrichDiagnosticContext = (diagnosticContext, context) =>
        {
            diagnosticContext.Set("TraceId", context.TraceIdentifier);
            diagnosticContext.Set("KeycloakUserId", context.User.FindFirstValue(KeycloakClaimNames.Subject));

            var currentIdentity = context.RequestServices
                .GetRequiredService<ICurrentUserAccessor>()
                .User;

            if (currentIdentity is not null)
            {
                diagnosticContext.Set("UserId", currentIdentity.Id);
            }
        };
    });

    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<CurrentIdentityMiddleware>();
    app.MapControllers();
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("live"),
        ResponseWriter = HealthCheckResponseWriter.WriteAsync,
    });
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready"),
        ResponseWriter = HealthCheckResponseWriter.WriteAsync,
    });
    app.Run();
}
catch (HostAbortedException)
{
    // EF Core design-time tools intentionally stop the temporary application host.
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
