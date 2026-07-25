namespace TestPlatform.Application.Users;

/// <summary>
/// Identity of the authenticated local user available to application use cases.
/// It deliberately contains no JWT claims or identity-provider details.
/// </summary>
public sealed record CurrentIdentity(
    Guid Id,
    string EmployeeNumber,
    bool IsAdmin);
