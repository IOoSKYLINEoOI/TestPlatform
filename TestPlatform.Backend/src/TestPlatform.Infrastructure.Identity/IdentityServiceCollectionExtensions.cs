using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TestPlatform.Application.Users;
using TestPlatform.Infrastructure.Identity.Management;

namespace TestPlatform.Infrastructure.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddTestPlatformIdentity(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = configuration.GetValue<bool?>("Authentication:RequireHttpsMetadata")
                    ?? !isDevelopment;
                options.Audience = Required(configuration, "Authentication:Audience");
                options.MetadataAddress = Required(configuration, "Authentication:MetadataAddress");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = Required(configuration, "Authentication:ValidIssuer"),
                    NameClaimType = KeycloakClaimNames.PreferredUsername,
                    RoleClaimType = KeycloakClaimNames.Roles,
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        services.AddOptions<KeycloakManagementOptions>()
            .Bind(configuration.GetSection(KeycloakManagementOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "Keycloak base URL is invalid.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Realm), "Keycloak realm is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "Keycloak management client ID is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientSecret), "Keycloak management client secret is required.")
            .ValidateOnStart();

        services.AddHttpClient<IIdentityAccountProvisioner, KeycloakIdentityAccountProvisioner>(
            (provider, client) =>
            {
                var options = provider.GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<KeycloakManagementOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        return services;
    }

    private static string Required(IConfiguration configuration, string key) =>
        configuration[key] ?? throw new InvalidOperationException($"Configuration value '{key}' is required.");
}
