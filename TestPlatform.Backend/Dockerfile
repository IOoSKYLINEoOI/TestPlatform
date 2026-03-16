# Stage build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем solution
COPY *.sln ./

# Копируем проекты для кэширования зависимостей
COPY src/TestPlatform.Application/*.csproj ./src/TestPlatform.Application/
COPY src/TestPlatform.Contracts/*.csproj ./src/TestPlatform.Contracts/
COPY src/TestPlatform.Core/*.csproj ./src/TestPlatform.Core/
COPY src/TestPlatform.Infrastructure.FileStorage/*.csproj ./src/TestPlatform.Infrastructure.FileStorage/
COPY src/TestPlatform.Infrastructure.Postgres/*.csproj ./src/TestPlatform.Infrastructure.Postgres/
COPY src/TestPlatform.Presenters/*.csproj ./src/TestPlatform.Presenters/
COPY src/TestPlatform.Web/*.csproj ./src/TestPlatform.Web/

# Восстанавливаем зависимости
RUN dotnet restore TestPlatform.sln

# Копируем весь исходный код
COPY src/ ./src/

# Публикация Web проекта
RUN dotnet publish src/TestPlatform.Web/TestPlatform.Web.csproj -c Release -o /app/publish

# Stage runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "TestPlatform.Web.dll"]
