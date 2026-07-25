using Microsoft.OpenApi;

namespace TestPlatform.Web.OpenApi;

public static class OpenApiServiceCollectionExtensions
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

            o.UseOneOfForPolymorphism();
            o.UseAllOfForInheritance();
            o.OperationFilter<TestPlatform.Web.OpenApi.QuestionRequestExamplesOperationFilter>();
            o.OperationFilter<ApiDocumentationOperationFilter>();
            o.SelectDiscriminatorNameUsing(_ => "kind");
            o.SelectDiscriminatorValueUsing(type => type.Name switch
            {
                var name when name.StartsWith("ChoiceQuestion", StringComparison.OrdinalIgnoreCase) => "choice",
                var name when name.StartsWith("TextQuestion", StringComparison.OrdinalIgnoreCase) => "text",
                var name when name.StartsWith("NumberQuestion", StringComparison.OrdinalIgnoreCase) => "number",
                var name when name.StartsWith("MatchingQuestion", StringComparison.OrdinalIgnoreCase) => "matching",
                _ => type.Name,
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
}
