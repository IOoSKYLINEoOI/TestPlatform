$ErrorActionPreference = 'Stop'

$keycloakUrl = 'http://keycloak:8080'
$realm = 'test-platform'
$tokenBody = @{
    grant_type = 'password'
    client_id = 'admin-cli'
    username = $env:KEYCLOAK_ADMIN
    password = $env:KEYCLOAK_ADMIN_PASSWORD
}
$accessToken = (Invoke-RestMethod -Method Post -Uri "$keycloakUrl/realms/master/protocol/openid-connect/token" -Body $tokenBody).access_token
$headers = @{ Authorization = "Bearer $accessToken" }
$users = @{
    'demo.admin' = 'DEMO-ADMIN'
    'demo.teacher' = 'DEMO-TEACHER-LOGIN'
    'demo.employee' = 'DEMO-EMPLOYEE'
}

foreach ($entry in $users.GetEnumerator()) {
    $result = @(Invoke-RestMethod -Headers $headers -Uri "$keycloakUrl/admin/realms/$realm/users?username=$($entry.Key)&exact=true")
    if ($result.Count -ne 1) {
        throw "Expected exactly one Keycloak user '$($entry.Key)', found $($result.Count)."
    }

    $body = @{
        username = $entry.Key
        enabled = $true
        attributes = @{ employee_number = @($entry.Value) }
    } | ConvertTo-Json -Depth 5

    Invoke-RestMethod -Method Put -Headers $headers -ContentType 'application/json' `
        -Uri "$keycloakUrl/admin/realms/$realm/users/$($result[0].id)" -Body $body
    Write-Host "Keycloak attribute for '$($entry.Key)' is ready."
}
