param(
    [string]$KeycloakUrl = "http://localhost:8080",
    [string]$Realm = "test-platform"
)

$envFile = Join-Path $PSScriptRoot "..\..\.env"
if (-not (Test-Path $envFile)) {
    throw "Create .env from .env.example before configuring Keycloak."
}

$environment = Get-Content $envFile | ConvertFrom-StringData
$tokenRequest = @{
    grant_type = "password"
    client_id = "admin-cli"
    username = $environment.KEYCLOAK_ADMIN
    password = $environment.KEYCLOAK_ADMIN_PASSWORD
}
$accessToken = (Invoke-RestMethod -Method Post -Uri "$KeycloakUrl/realms/master/protocol/openid-connect/token" -Body $tokenRequest).access_token
$headers = @{ Authorization = "Bearer $accessToken" }

$profile = Invoke-RestMethod -Headers $headers -Uri "$KeycloakUrl/admin/realms/$Realm/users/profile"
$profile.attributes = @($profile.attributes | Where-Object { $_.name -ne "employee_number" }) + @{
    name = "employee_number"
    displayName = "Employee number"
    validations = @{ length = @{ min = 1; max = 64 } }
    required = @{ roles = @("admin", "user") }
    permissions = @{ view = @("admin"); edit = @("admin") }
    multivalued = $false
}

$body = $profile | ConvertTo-Json -Depth 20
Invoke-RestMethod -Method Put -Headers $headers -ContentType "application/json" -Uri "$KeycloakUrl/admin/realms/$Realm/users/profile" -Body $body
Write-Host "User Profile configured: employee_number is required and can only be viewed/edited by administrators."
