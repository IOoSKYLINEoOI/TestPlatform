# Keycloak

`test-platform-realm.json` is imported only when the `test-platform` realm does not yet exist.

To create an employee, an administrator creates a Keycloak user with:

- a unique `username` used only for login;
- a temporary password and the required `Update Password` action;
- the `employee_number` user attribute;
- one or more realm roles: `Employee`, `Teacher`, `Admin`.

`employee_number` is the business identifier. It must not be used as the Keycloak username and must not be editable by the employee.

The `test-platform-admin` confidential client is used by the API to provision
employee accounts. Its service account has only `manage-users`, `query-users`,
and `view-users` realm-management roles. Its secret comes from
`KEYCLOAK_ADMIN_CLIENT_SECRET`; never commit the real value.

`bootstrap-demo-users.sh` runs in a separate one-shot container and creates
`demo.admin`, `demo.teacher`, and `demo.employee`. It waits for Keycloak,
updates accounts idempotently through `kcadm`, assigns `employee_number` and a
realm role, applies the `mytheme` login theme, disables the profile-completion
prompt, and reads all passwords from environment variables.

Only the login and password are entered during authentication. A newly
provisioned employee still has to replace the temporary password on the first
login. This is intentional and must not be confused with profile completion.

The User Profile schema is configured through Keycloak's separate Admin API and is not part of the Realm representation import. After the first startup (and after a Keycloak database reset), run:

```powershell
powershell -ExecutionPolicy Bypass -File .\containers\keycloak\configure-user-profile.ps1
```

It makes `employee_number` a required, single-value attribute visible and editable only to administrators. Add an organisation-specific validation pattern in the script when its employee-number format is settled. The application also rejects a token without this claim and rejects a changed number, so a partially configured account cannot access the API.

For local development, reset the Keycloak database only when realm data can be discarded:

```powershell
docker compose down
docker volume rm testplatformbackend_keycloak_pgdata
docker compose up --build
```

For a production deployment use `start`, HTTPS, a stable public hostname and secret values provided outside the repository.
