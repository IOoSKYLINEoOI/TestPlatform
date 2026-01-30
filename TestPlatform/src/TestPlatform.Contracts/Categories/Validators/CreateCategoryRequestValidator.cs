using FluentValidation;
using TestPlatform.Contracts.Categories.DTOs;

namespace TestPlatform.Contracts.Categories.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    private const int MaxLengthName = 100;
    private const int MaxLengthDescription = 250;

    public CreateCategoryRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Название обязательно.")
            .MaximumLength(MaxLengthName).WithMessage("Название не должно привышать 100 символов.");

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage("Описание категории обязательно.")
            .MaximumLength(MaxLengthDescription).WithMessage("Описание категории не должно превышать 250 символов.");
    }
}