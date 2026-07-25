using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Users;

public class User
{
    public const int MaxKeycloakIdLength = 255;
    public const int MaxEmployeeNumberLength = 50;

    private User() { }

    private User(Guid id, string keycloakId, string employeeNumber)
    {
        Id = id;
        KeycloakId = keycloakId;
        EmployeeNumber = employeeNumber;
    }

    public Guid Id { get; }

    public string KeycloakId { get; } = null!;

    public string EmployeeNumber { get; } = null!;

    public static Result<User> Create(string keycloakId, string employeeNumber)
    {
        var validation = Validate(keycloakId, employeeNumber);
        if (validation.IsFailure)
        {
            return Result.Failure<User>(validation.Error);
        }

        return Result.Success(new User(
            Guid.NewGuid(),
            keycloakId.Trim(),
            employeeNumber.Trim()));
    }

    private static Result Validate(string keycloakId, string employeeNumber)
    {
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Result.Failure("user.keycloak_id_required");
        }

        if (keycloakId.Trim().Length > MaxKeycloakIdLength)
        {
            return Result.Failure("user.keycloak_id_too_long");
        }

        if (string.IsNullOrWhiteSpace(employeeNumber))
        {
            return Result.Failure("user.employee_number_required");
        }

        if (employeeNumber.Trim().Length > MaxEmployeeNumberLength)
        {
            return Result.Failure("user.employee_number_too_long");
        }

        return Result.Success();
    }
}
