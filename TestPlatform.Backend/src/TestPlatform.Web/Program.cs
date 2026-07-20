using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using TestPlatform.Application;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Users.DTOs;
using TestPlatform.Infrastructure.Files.ImageProcessing;
using TestPlatform.Infrastructure.Identity;
using TestPlatform.Infrastructure.Files.ObjectStorage.Minio;
using TestPlatform.Infrastructure.Postgres;
using TestPlatform.Web.Extensions;
using TestPlatform.Web.Middleware;

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

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGenWithAuthSupport(builder.Configuration);

    builder.Services.AddAuthorization();

    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"]!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],

            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role,
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Authentication failed.");
                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                var identity = (ClaimsIdentity)context.Principal!.Identity!;
                var sub = context.Principal!.FindFirst("sub")?.Value;
                if (sub != null)
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));

                foreach (var role in context.Principal.FindAll("role"))
                {
                    if (!identity.HasClaim(ClaimTypes.Role, role.Value))
                        identity.AddClaim(new Claim(ClaimTypes.Role, role.Value));
                }

                return Task.CompletedTask;
            },
        };
    });

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

    builder.Services.AddControllers(config =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    config.Filters.Add(new AuthorizeFilter(policy));
    });

    builder.Services
    .AddTestPlatformPersistence(builder.Configuration)
    .AddTestPlatformApplication()
    .AddTestPlatformMinioObjectStorage(builder.Configuration)
    .AddTestPlatformImageProcessing(builder.Configuration)
        .AddCurrentUser();

    builder.Services.AddProblemDetails();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5062);
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
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
                return LogEventLevel.Error;

            return context.Response.StatusCode >= StatusCodes.Status400BadRequest
                ? LogEventLevel.Warning
                : LogEventLevel.Information;
        };

        options.EnrichDiagnosticContext = (diagnosticContext, context) =>
        {
            diagnosticContext.Set("TraceId", context.TraceIdentifier);
            diagnosticContext.Set("KeycloakUserId", context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (context.Items["CurrentUser"] is CurrentUserDto currentUser)
                diagnosticContext.Set("UserId", currentUser.Id);
        };
    });

    app.UseCors("Frontend");

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<EnsureUserMiddleware>();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
