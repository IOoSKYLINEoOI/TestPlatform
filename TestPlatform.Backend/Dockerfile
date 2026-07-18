FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.sln ./
COPY src/TestPlatform.Application/*.csproj ./src/TestPlatform.Application/
COPY src/TestPlatform.Contracts/*.csproj ./src/TestPlatform.Contracts/
COPY src/TestPlatform.Core/*.csproj ./src/TestPlatform.Core/
COPY src/TestPlatform.Infrastructure.FileStorage/*.csproj ./src/TestPlatform.Infrastructure.FileStorage/
COPY src/TestPlatform.Infrastructure.Postgres/*.csproj ./src/TestPlatform.Infrastructure.Postgres/
COPY src/TestPlatform.Infrastructure.Identity/*.csproj ./src/TestPlatform.Infrastructure.Identity/
COPY src/TestPlatform.Presenters/*.csproj ./src/TestPlatform.Presenters/
COPY src/TestPlatform.Web/*.csproj ./src/TestPlatform.Web/

RUN dotnet restore TestPlatform.sln

COPY src/ ./src/

RUN dotnet publish src/TestPlatform.Web/TestPlatform.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "TestPlatform.Web.dll"]
