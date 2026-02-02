using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Categories.Validators;

namespace TestPlatform.Application;

public static class TestPlatformApplicationExtensions
{
    public static IServiceCollection AddTestPlatformApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();

        var assembly = typeof(CreateCategoryRequestValidator).Assembly;

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}