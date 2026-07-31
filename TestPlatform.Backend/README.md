# TestPlatform Backend

Backend учебной платформы: API для управления вопросами, тегами, тестами и экзаменами, прохождения попыток, загрузки изображений и управления учётными записями сотрудников.

Документация проекта ведётся на русском языке. Контракт HTTP API доступен в Swagger после локального запуска.

## Технологии

- .NET 10 / ASP.NET Core, EF Core и PostgreSQL;
- Keycloak и JWT-аутентификация;
- MinIO для изображений, Seq для структурированных логов;
- Docker Compose для локального окружения;
- xUnit: модульные и интеграционные тесты.

## Состав решения

| Проект | Назначение |
| --- | --- |
| `src/TestPlatform.Web` | Точка входа, HTTP-конвейер, Swagger, health checks и фоновые задачи. |
| `src/TestPlatform.Api` | Контроллеры HTTP API. |
| `src/TestPlatform.Application` | Прикладные сценарии и бизнес-координация. |
| `src/TestPlatform.Core` | Предметная модель и бизнес-правила. |
| `src/TestPlatform.Contracts` | DTO и контракты API. |
| `src/TestPlatform.Infrastructure.*` | PostgreSQL, Keycloak и файловое хранилище. |
| `tests/*` | Модульные и интеграционные тесты. |

## Быстрый старт в Docker

Файл Compose находится в корне проекта `TestPlatform`, на один уровень выше этой папки. Все команды этого раздела выполняются из этого корня.

1. Создайте файл окружения рядом с `docker-compose.yml`:

   ```powershell
   Copy-Item .\TestPlatform.Backend\.env.example .\.env
   ```

2. Замените все примерные пароли и `KEYCLOAK_ADMIN_CLIENT_SECRET` в `.env` на уникальные значения.

3. Запустите окружение:

   ```powershell
   docker compose up --build
   ```

Compose последовательно поднимает PostgreSQL, применяет миграции в одноразовом контейнере `migrations`, создаёт демонстрационные данные в контейнере `seed`, настраивает Keycloak и только затем запускает API и административный интерфейс. Одноразовые контейнеры должны завершиться с кодом `0`.

| Сервис | Адрес |
| --- | --- |
| API / Swagger (Development) | http://localhost:5062/swagger |
| API liveness | http://localhost:5062/health/live |
| API readiness | http://localhost:5062/health/ready |
| Администрирование Keycloak | http://localhost:8080/admin |
| Seq | http://localhost:8081 |
| Консоль MinIO | http://localhost:9101 |
| Admin UI | http://localhost:5176 |

Swagger включён только в окружении `Development`. Все контроллеры по умолчанию требуют аутентифицированного пользователя; исключение — health checks.

## Локальный запуск без Docker

Для запуска приложения требуются доступные PostgreSQL, Keycloak и MinIO. Seq подключается при наличии непустого `Seq:ServerUrl`; без него приложение продолжит писать логи в консоль. Параметры подключения задаются в `appsettings.Development.json`, переменных окружения или User Secrets. Секреты не добавляйте в `appsettings.json`.

```powershell
dotnet restore
dotnet run --project .\src\TestPlatform.Web
```

Для provisioning пользователей сохраните секрет сервисного клиента Keycloak локально:

```powershell
dotnet user-secrets set "IdentityManagement:ClientSecret" "<strong-secret>" `
  --project .\src\TestPlatform.Web
```

Разрешённые CORS-источники: `http://localhost:5175` и `http://localhost:5176`.

## Миграции и демонстрационные данные

Создать миграцию после изменения модели EF Core:

```powershell
dotnet ef migrations add MigrationName `
  --project .\src\TestPlatform.Infrastructure.Postgres `
  --startup-project .\src\TestPlatform.Web
```

Применить миграции без запуска HTTP-сервера:

```powershell
dotnet run --project .\src\TestPlatform.Web -- --migrate
```

Создать демонстрационные данные:

```powershell
dotnet run --project .\src\TestPlatform.Web -- --seed
```

Команда `--seed` доступна только в `Development`, сначала применяет миграции и выполняется идемпотентно. Не используйте `EnsureCreated` для прикладной PostgreSQL-базы: этот метод обходит историю миграций. Он применяется только в SQLite-фикстуре интеграционных тестов.

Seed создаёт 20 пользователей, 12 тегов, 150 вопросов, 20 тренировочных тестов, 8 экзаменов и 240 попыток в разных состояниях.

## Демонстрационные учётные записи

Пароли задаются в `.env` переменными `DEMO_*_PASSWORD` перед `docker compose up`.

| Пользователь | Табельный номер | Роль |
| --- | --- | --- |
| `demo.admin` | `DEMO-ADMIN` | `Admin` |
| `demo.teacher` | `DEMO-TEACHER-LOGIN` | `Teacher` |
| `demo.employee` | `DEMO-EMPLOYEE` | `Employee` |

Скрипт bootstrap обновляет существующие demo-аккаунты, не создавая дубликатов. Учётные записи `seed:*` используются только как владельцы контента и истории попыток; вход в них невозможен.

## Роли и API

В системе определены роли `Admin`, `Teacher` и `Employee`.

- `Admin` управляет системой, пользователями и всем контентом;
- `Teacher` создаёт и редактирует учебный контент и теги;
- `Employee` проходит доступные тесты и экзамены.

Основные группы ресурсов: `/questions`, `/tags`, `/tests`, `/exams`, `/attempts`, `/images` и `/users`. Полный перечень запросов, схем и требований авторизации приведён в Swagger.

Администратор создаёт сотрудника запросом `POST /users`:

```json
{
  "username": "employee.login",
  "employeeNumber": "EMP-001",
  "temporaryPassword": "Temporary-Password-123!",
  "role": "Employee"
}
```

Имя пользователя может состоять из латинских букв, цифр, точек, подчёркиваний и дефисов. Пароль выдаётся как временный: Keycloak потребует заменить его при первом входе.

Подробности настройки Keycloak и профиля пользователя: [`containers/keycloak/README.md`](containers/keycloak/README.md).

## Проверки

```powershell
dotnet test .\TestPlatform.sln
dotnet build .\TestPlatform.sln --configuration Release
```

## Полезные команды Docker

Выполняйте из корня `TestPlatform`:

```powershell
# Остановить окружение, сохранив тома с данными
docker compose down

# Просмотреть состояние контейнеров
docker compose ps

# Просмотреть логи API
docker compose logs --tail 100 api
```

Импорт realm в Keycloak выполняется только при создании нового realm. Если нужно заново применить импорт в локальной среде без ценных данных, удалите соответствующий том Keycloak согласно инструкции в [`containers/keycloak/README.md`](containers/keycloak/README.md).
