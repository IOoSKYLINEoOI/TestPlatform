using FluentValidation;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Questions.Tags.Validators;

public class TagRequestValidator : AbstractValidator<TagRequest>
{
    private const int MaxLengthName = 100;
    private const int MaxLengthDescription = 250;

    public TagRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Название обязательно.")
            .MaximumLength(MaxLengthName).WithMessage("Название не должно привышать 100 символов.");

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage("Описание категории обязательно.")
            .MaximumLength(MaxLengthDescription).WithMessage("Описание категории не должно превышать 250 символов.");
    }
}
