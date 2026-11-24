# ---------- Етап 1: збирання (SDK, ~800 МБ) ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Спочатку лише файли проєктів: шар restore кешується, доки не змінено залежності
COPY ItOpsMonitor.Domain/ItOpsMonitor.Domain.csproj ItOpsMonitor.Domain/
COPY ItOpsMonitor.Infrastructure/ItOpsMonitor.Infrastructure.csproj ItOpsMonitor.Infrastructure/
COPY ItOpsMonitor.API/ItOpsMonitor.API.csproj ItOpsMonitor.API/
RUN dotnet restore ItOpsMonitor.API/ItOpsMonitor.API.csproj

# Потім вихідний код (WPF-клієнт і тести не потрібні: їх відсікає .dockerignore)
COPY ItOpsMonitor.Domain/ ItOpsMonitor.Domain/
COPY ItOpsMonitor.Infrastructure/ ItOpsMonitor.Infrastructure/
COPY ItOpsMonitor.API/ ItOpsMonitor.API/
RUN dotnet publish ItOpsMonitor.API/ItOpsMonitor.API.csproj \
    -c Release -o /app --no-restore /p:UseAppHost=false

# ---------- Етап 2: виконання (лише ASP.NET Core runtime, ~220 МБ) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

# Непривілейований користувач, передбачений образом .NET 8
USER $APP_UID
ENTRYPOINT ["dotnet", "ItOpsMonitor.API.dll"]
