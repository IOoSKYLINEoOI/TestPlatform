

using Microsoft.OpenApi;
using TestPlatform.Application.Abstractions;
using TestPlatform.Infrastructure.Identity;

namespace TestPlatform.Web.Extensions;

public static class TestPlatformWebApiExtensions
{
    public static IServiceCollection AddSwaggerGenWithAuthSupport(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo { Title = "TestPlatform", Version = "v1" });

            o.EnableAnnotations();

            o.UseInlineDefinitionsForEnums();

            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            o.AddSecurityDefinition(
                "Keycloak",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(configuration["Keycloak:AuthorizationUrl"]!),
                            TokenUrl = new Uri(configuration["Keycloak:TokenUrl"]!),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "openid" },
                                { "profile", "profile" },
                            },
                        },
                    },
                });

            o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecuritySchemeReference("Keycloak", doc),
                    []
                },
            });
        });
        return services;
    }

    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}