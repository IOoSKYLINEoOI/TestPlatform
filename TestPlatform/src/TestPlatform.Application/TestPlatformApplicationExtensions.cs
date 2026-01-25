using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Contracts.Categories.Validators;

namespace TestPlatform.Application;

public static class TestPlatformApplicationExtensions
{
    public static IServiceCollection AddTestPlatformApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CategoryRequestValidator>();

        return services;
    }
}