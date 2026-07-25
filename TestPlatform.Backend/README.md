# TestPlatform Backend

## Local start with Docker

1. Copy `.env.example` to `.env` and replace the example secrets.
2. Start the complete environment:

   ```powershell
   docker compose up --build
   ```

Docker Compose starts PostgreSQL first, runs all application migrations in the
one-shot `migrations` container, and only then starts the API. The migration
container is expected to finish with exit code `0`; it is not a long-running
service.

Local addresses:

- API and Swagger: `http://localhost:5062/swagger`
- API liveness: `http://localhost:5062/health/live`
- API readiness: `http://localhost:5062/health/ready`
- Keycloak administration: `http://localhost:8080/admin`
- Seq: `http://localhost:8081`
- MinIO console: `http://localhost:9101`

## Database migrations

Create a migration after changing the EF Core model:

```powershell
dotnet ef migrations add MigrationName `
  --project src/TestPlatform.Infrastructure.Postgres `
  --startup-project src/TestPlatform.Web
```

Apply migrations without starting the HTTP server:

```powershell
dotnet run --project src/TestPlatform.Web -- --migrate
```

Do not use `EnsureCreated` for the application database. It bypasses migration
history and is only used by the SQLite integration-test fixture.

## Development seed data

Docker Compose runs the one-shot `seed` container after migrations and before
the API. It creates an idempotent connected demo dataset:

- 20 demo users, including one content author;
- 12 tags;
- 150 questions with text, number, single-choice, and multiple-choice answers;
- 20 practice tests with published and draft examples;
- 8 exams with 30-question pools and stable five-question scoring;
- 240 attempts distributed across finished, active, expired, abandoned, and
  not-started states.

Run the same seed manually:

```powershell
dotnet run --project src/TestPlatform.Web -- --seed
```

The command applies pending migrations first and is rejected outside the
`Development` environment. Repeated execution detects the seed marker user and
does not create duplicates.

### Demo logins

The one-shot `keycloak-bootstrap` container creates three idempotent Keycloak
accounts after the realm is available:

| Username | Employee number | Role |
| --- | --- | --- |
| `demo.admin` | `DEMO-ADMIN` | `Admin` |
| `demo.teacher` | `DEMO-TEACHER-LOGIN` | `Teacher` |
| `demo.employee` | `DEMO-EMPLOYEE` | `Employee` |

Set their passwords in `.env` before starting Docker:

```dotenv
DEMO_ADMIN_PASSWORD=replace-with-a-strong-demo-password
DEMO_TEACHER_PASSWORD=replace-with-a-strong-demo-password
DEMO_EMPLOYEE_PASSWORD=replace-with-a-strong-demo-password
```

The bootstrap script updates existing demo accounts instead of duplicating
them. Passwords are intentionally not stored in the realm export, source code,
or logs. The `seed:*` PostgreSQL users are synthetic owners for content and
attempt history; they are not login accounts.

## Employee account provisioning

Set a strong random value for `KEYCLOAK_ADMIN_CLIENT_SECRET` in `.env`. The same
value is injected into the Keycloak service-account client and into the API.
For a non-Docker API start, store it outside `appsettings.json`:

```powershell
dotnet user-secrets set "IdentityManagement:ClientSecret" "replace-with-a-strong-secret" `
  --project src/TestPlatform.Web
```

An administrator can create an account through:

```http
POST /users
```

```json
{
  "username": "employee.login",
  "employeeNumber": "EMP-001",
  "temporaryPassword": "Temporary-Password-123!",
  "role": "Employee"
}
```

Usernames accept Latin letters, digits, dots, underscores, and hyphens. The
password is marked as temporary, so Keycloak requires the employee to replace
it during the first login. Allowed realm roles are `Admin`, `Teacher`, and
`Employee`; only an existing `Admin` may call this endpoint.

Realm import is applied only when Keycloak initializes a new realm. If the
`test-platform` realm already exists in the Keycloak PostgreSQL volume, adding
the service-account client to the JSON export does not modify that existing
realm. For a development environment without valuable data, recreate the
containers and volumes before testing the new provisioning endpoint.
