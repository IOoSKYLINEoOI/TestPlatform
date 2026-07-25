using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TestPlatform.Api.Common.Validation;

public sealed class FluentValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values.Where(ShouldValidate))
        {
            var argumentType = argument!.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContextType = typeof(ValidationContext<>).MakeGenericType(argumentType);
            var validationContext = (IValidationContext)Activator.CreateInstance(
                validationContextType,
                argument)!;
            var result = await validator.ValidateAsync(
                validationContext,
                context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                context.Result = new BadRequestObjectResult(result.ToValidationProblemDetails());
                return;
            }
        }

        await next();
    }

    private static bool ShouldValidate(object? argument)
    {
        if (argument is null)
        {
            return false;
        }

        var type = argument.GetType();
        return !type.IsPrimitive
            && type != typeof(string)
            && type != typeof(Guid)
            && type != typeof(CancellationToken);
    }
}
