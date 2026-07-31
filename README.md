# TestPlatform

Локальная платформа для создания тестов и экзаменов, прохождения попыток и управления результатами через административную панель.

## Состав проекта

- `TestPlatform.Backend` — ASP.NET Core API, PostgreSQL, миграции и интеграционные тесты.
- `TestPlatform.Admin` — React/Vite административная панель.
- `docker-compose.yml` — полный локальный стенд: API, Admin, PostgreSQL, Keycloak, MinIO и Seq.

## Требования

- Docker Desktop с Docker Compose.
- Для запуска без Docker: .NET SDK и Node.js 22+ с pnpm.

## Быстрый запуск

1. Скопируйте `.env.example` в `.env`.
2. Замените значения `change-...` в `.env` локальными паролями.
3. Запустите стенд:

```powershell
docker compose up --build
```

При первом запуске Compose применит миграции, добавит демонстрационные данные и настроит пользователей Keycloak.

## Локальные адреса

- Административная панель: http://localhost:5176
- API и Swagger: http://localhost:5062/swagger/index.html
- Keycloak: http://localhost:8080
- Seq: http://localhost:8081
- MinIO Console: http://localhost:9101

Пароли демонстрационных пользователей задаются переменными `DEMO_ADMIN_PASSWORD`, `DEMO_TEACHER_PASSWORD` и `DEMO_EMPLOYEE_PASSWORD` в `.env`. Имена пользователей и назначение ролей описаны в `TestPlatform.Backend/containers/keycloak/README.md`.

## Проверки

Backend:

```powershell
dotnet test TestPlatform.Backend/TestPlatform.sln
```

Frontend:

```powershell
cd TestPlatform.Admin
corepack enable
pnpm install --frozen-lockfile
pnpm test
pnpm build
```

E2E для запущенного локального стенда:

```powershell
cd TestPlatform.Admin
pnpm exec playwright install chromium
pnpm test:e2e
```

## Остановка

```powershell
docker compose down
```

Данные PostgreSQL, Keycloak, MinIO и Seq сохраняются в Docker volumes. Для обычного перезапуска удалять volumes не требуется.
