using TestPlatform.Core.Users;
using Xunit;

namespace TestPlatform.Core.Tests.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_NormalizesIdentityValues()
    {
        var result = User.Create("  keycloak-sub  ", "  EMP-001  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("keycloak-sub", result.Value.KeycloakId);
        Assert.Equal("EMP-001", result.Value.EmployeeNumber);
    }

    [Fact]
    public void Create_RejectsValuesThatDoNotFitPersistenceLimits()
    {
        var longKeycloakId = new string('a', User.MaxKeycloakIdLength + 1);
        var longEmployeeNumber = new string('1', User.MaxEmployeeNumberLength + 1);

        var keycloakResult = User.Create(longKeycloakId, "EMP-001");
        var employeeResult = User.Create("keycloak-sub", longEmployeeNumber);

        Assert.True(keycloakResult.IsFailure);
        Assert.Equal("user.keycloak_id_too_long", keycloakResult.Error);
        Assert.True(employeeResult.IsFailure);
        Assert.Equal("user.employee_number_too_long", employeeResult.Error);
    }
}
