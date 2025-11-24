FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /src

COPY ["Chibest.sln", "./"]
COPY ["Chibest.API/Chibest.API.csproj", "Chibest.API/"]
COPY ["Chibest.Service/Chibest.Service.csproj", "Chibest.Service/"]
COPY ["Chibest.Repository/Chibest.Repository.csproj", "Chibest.Repository/"]
COPY ["Chibest.Common/Chibest.Common.csproj", "Chibest.Common/"]

RUN dotnet restore "Chibest.API/Chibest.API.csproj"

COPY . .
RUN dotnet publish "Chibest.API/Chibest.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

RUN groupadd --system --gid 2000 chibest \
    && useradd --system --create-home --uid 2000 --gid chibest chibest

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    AUTO_APPLY_MIGRATIONS=false

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

USER chibest

ENTRYPOINT ["dotnet", "Chibest.API.dll"]


