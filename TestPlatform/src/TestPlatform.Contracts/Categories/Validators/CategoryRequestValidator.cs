using FluentValidation;
using TestPlatform.Contracts.Categories.DTOs;

namespace TestPlatform.Contracts.Categories.Validators;

public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Название обязательно.")
            .MaximumLength(100).WithMessage("Название не должно привышать 100 символов.");

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage("Описание категории обязательно.")
            .MaximumLength(250).WithMessage("Описание категории не должно превышать 250 символов.");
    }
}