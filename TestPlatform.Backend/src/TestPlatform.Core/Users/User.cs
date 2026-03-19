using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Users;

public class User
{
    private User(Guid id, string keycloakId, string tabNumber)
    {
        Id = id;
        KeycloakId = keycloakId;
        TabNumber = tabNumber;
    }

    public Guid Id { get; }

    public string KeycloakId { get; }

    public string TabNumber { get; }

    public static Result<User> Create(string keycloakId, string tabNumber)
    {
        var validation = Validate(keycloakId, tabNumber);
        if (validation.IsFailure)
            return Result.Failure<User>(validation.Error);

        return Result.Success(new User(Guid.NewGuid(), keycloakId, tabNumber));
    }

    private static Result Validate(string keycloakId, string tabNumber)
    {
        if (string.IsNullOrWhiteSpace(keycloakId))
            return Result.Failure("KeycloakId is required");
        if (string.IsNullOrWhiteSpace(tabNumber))
            return Result.Failure("TabNumber is required");
        return Result.Success();
    }
}