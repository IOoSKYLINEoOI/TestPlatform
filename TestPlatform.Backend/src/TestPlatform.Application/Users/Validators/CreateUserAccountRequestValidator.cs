using FluentValidation;
using TestPlatform.Contracts.Users.DTOs;
using TestPlatform.Core.Users;

namespace TestPlatform.Application.Users.Validators;

public sealed class CreateUserAccountRequestValidator
    : AbstractValidator<CreateUserAccountRequest>
{
    private static readonly string[] AllowedRoles = ["Admin", "Teacher", "Employee"];

    public CreateUserAccountRequestValidator()
    {
        RuleFor(request => request.Username)
            .NotEmpty()
            .Length(3, 64)
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Username may contain only Latin letters, digits, dots, underscores, and hyphens.");

        RuleFor(request => request.EmployeeNumber)
            .NotEmpty()
            .MaximumLength(User.MaxEmployeeNumberLength);

        RuleFor(request => request.TemporaryPassword)
            .NotEmpty()
            .Length(12, 128);

        RuleFor(request => request.Role)
            .Must(role => AllowedRoles.Contains(role, StringComparer.Ordinal))
            .WithMessage("Role must be Admin, Teacher, or Employee.");
    }
}
