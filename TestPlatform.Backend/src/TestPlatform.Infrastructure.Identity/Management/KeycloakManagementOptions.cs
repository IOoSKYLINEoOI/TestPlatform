namespace TestPlatform.Infrastructure.Identity.Management;

public sealed class KeycloakManagementOptions
{
    public const string SectionName = "IdentityManagement";

    public string BaseUrl { get; set; } = string.Empty;

    public string Realm { get; set; } = "test-platform";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}
