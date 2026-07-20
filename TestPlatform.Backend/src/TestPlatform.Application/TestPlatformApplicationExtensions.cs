using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Application.Attempts.Services;
using TestPlatform.Application.Attempts.Services.SourceService;
using TestPlatform.Application.Exams.Services;
using TestPlatform.Application.Files;
using TestPlatform.Application.Questions.Tags.Validators;
using TestPlatform.Application.Tests.Services;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Exams;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application;

public static class TestPlatformApplicationExtensions
{
    public static IServiceCollection AddTestPlatformApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<TagRequestValidator>();
        var assembly = typeof(TestPlatformApplicationExtensions).Assembly;

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

        services.AddScoped<IAttemptSourceService, TestAttemptSource>();
        services.AddScoped<IAttemptSourceService, ExamAttemptSource>();
        services.AddScoped<AttemptSourceResolver>();

        services.AddScoped<IAccessService<Exam>, ExamAccessService>();
        services.AddScoped<IAccessService<Test>, TestAccessService>();
        services.AddScoped<IAccessService<Attempt>, AttemptAccessService>();
        services.AddScoped<IFileAssetService, FileAssetService>();

        return services;
    }
}
