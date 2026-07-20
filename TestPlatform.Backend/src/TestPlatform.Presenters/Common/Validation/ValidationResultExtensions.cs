using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace TestPlatform.Presenters.Common.Validation;

public static class ValidationResultExtensions
{
    public static ValidationProblemDetails ToValidationProblemDetails(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Ошибка валидации запроса.",
        };
    }
}
