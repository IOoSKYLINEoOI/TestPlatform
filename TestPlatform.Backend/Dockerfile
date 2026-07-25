FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .editorconfig ./
COPY src/TestPlatform.Application/*.csproj ./src/TestPlatform.Application/
COPY src/TestPlatform.Contracts/*.csproj ./src/TestPlatform.Contracts/
COPY src/TestPlatform.Core/*.csproj ./src/TestPlatform.Core/
COPY src/TestPlatform.Infrastructure.Files/*.csproj ./src/TestPlatform.Infrastructure.Files/
COPY src/TestPlatform.Infrastructure.Postgres/*.csproj ./src/TestPlatform.Infrastructure.Postgres/
COPY src/TestPlatform.Infrastructure.Identity/*.csproj ./src/TestPlatform.Infrastructure.Identity/
COPY src/TestPlatform.Api/*.csproj ./src/TestPlatform.Api/
COPY src/TestPlatform.Web/*.csproj ./src/TestPlatform.Web/

RUN dotnet restore src/TestPlatform.Web/TestPlatform.Web.csproj

COPY src/ ./src/

RUN dotnet publish src/TestPlatform.Web/TestPlatform.Web.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

HEALTHCHECK --interval=15s --timeout=5s --start-period=20s --retries=5 \
    CMD curl --fail --silent http://localhost:5062/health/live || exit 1

ENTRYPOINT ["dotnet", "TestPlatform.Web.dll"]
