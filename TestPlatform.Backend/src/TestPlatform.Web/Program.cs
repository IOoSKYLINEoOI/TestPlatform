using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TestPlatform.Application;
using TestPlatform.Infrastructure.FileStorage;
using TestPlatform.Infrastructure.Identity;
using TestPlatform.Infrastructure.Postgres;
using TestPlatform.Web.Extensions;
using TestPlatform.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();
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
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                var identity = (ClaimsIdentity)context.Principal?.Identity!;
                var claims = context.Principal?.Claims.ToList();

                if (claims != null)
                {
                    string? sub = claims.FirstOrDefault(x => x.Type == "sub")?.Value;
                    if (!string.IsNullOrEmpty(sub))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
                    }
                }

                if (claims != null)
                {
                    string? resourceAccess = claims
                        .FirstOrDefault(x => x.Type == "resource_access")?.Value;

                    if (!string.IsNullOrEmpty(resourceAccess))
                    {
                        try
                        {
                            var json = JsonDocument.Parse(resourceAccess);

                            if (json.RootElement.TryGetProperty("public-client", out var client))
                            {
                                if (client.TryGetProperty("roles", out var roles))
                                {
                                    foreach (var role in roles.EnumerateArray())
                                    {
                                        string? roleValue = role.GetString();
                                        if (!string.IsNullOrEmpty(roleValue))
                                        {
                                            identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to parse roles from resource_access");
                        }
                    }
                }

                logger.LogInformation(
                    "Token validated. UserId: {UserId}, Username: {Username}",
                    identity.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    context.Principal?.Identity?.Name);

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrOwner", policy =>
        policy.Requirements.Add(new AdminOrOwnerRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, AdminOrOwnerHandler>();

/*builder.Services.AddControllers(config =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});*/

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("TestPlatform"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        tracing.AddOtlpExporter();
    });

builder.Services
    .AddTestPlatformPersistence(builder.Configuration)
    .AddTestPlatformApplication()
    .AddTestPlatformFileStorage(builder.Configuration)
    .AddCurrentUser();

builder.Services.AddProblemDetails();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5062);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
    db.Database.Migrate();

    await DbInitializer.InitializeAsync(db);
}

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

app.UseCors(policy =>
{
    policy.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("http://localhost:5173");
});

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<EnsureUserMiddleware>();

app.MapControllers();

app.Run();