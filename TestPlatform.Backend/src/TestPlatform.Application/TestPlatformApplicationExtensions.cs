using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Questions.Validators;
using TestPlatform.Application.Tags.Validators;

namespace TestPlatform.Application;

public static class TestPlatformApplicationExtensions
{
    public static IServiceCollection AddTestPlatformApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateTagRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateQuestionRequestValidator>();

        var assembly = typeof(CreateTagRequestValidator).Assembly;

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