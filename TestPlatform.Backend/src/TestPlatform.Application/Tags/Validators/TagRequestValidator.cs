using FluentValidation;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Tags.Validators;

public class TagRequestValidator : AbstractValidator<TagRequest>
{
    private const int MaxLengthName = 100;
    private const int MaxLengthDescription = 250;

    public TagRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(MaxLengthName).WithMessage("Name must not exceed 100 characters.");

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(MaxLengthDescription).WithMessage("Description must not exceed 250 characters.");
    }
}
