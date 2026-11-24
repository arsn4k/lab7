using ItOpsMonitor.API;
using ItOpsMonitor.API.Health;
using ItOpsMonitor.Infrastructure;
using ItOpsMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. СУБД і рядок підключення — лише з конфігурації (appsettings / змінні середовища)
var provider = builder.Configuration.GetValue("Database:Provider", DatabaseProvider.Sqlite);

var rawConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(rawConnection))
    rawConnection = builder.Configuration["DATABASE_URL"]; // запасний варіант для Render
if (string.IsNullOrWhiteSpace(rawConnection))
    throw new InvalidOperationException(
        "Не задано рядок підключення: ConnectionStrings__DefaultConnection або DATABASE_URL.");

var connectionString = ConnectionStringResolver.Normalize(rawConnection, provider);

builder.Services.AddItOpsDatabase(provider, connectionString, builder.Environment.IsDevelopment());

// 2. Перевірки стану: з'єднання з БД і актуальність схеми
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ItOpsDbContext>("database")
    .AddCheck<MigrationsHealthCheck>("migrations");

// 3. CORS: дозволені домени клієнтської частини задаються в конфігурації
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    if (allowedOrigins.Length > 0)
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Міграції під час запуску (Render) — або вимкнути й застосувати dotnet ef database update
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ItOpsDbContext>();
    var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    log.LogInformation("Запуск ItOpsMonitor.API: середовище {Env}, СУБД {Provider}",
        app.Environment.EnvironmentName, provider);

    if (app.Configuration.GetValue("Database:MigrateOnStartup", true))
    {
        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
        log.LogInformation("Міграцій до застосування: {Count} [{List}]",
            pending.Count, string.Join(", ", pending));

        await db.Database.MigrateAsync();

        var last = (await db.Database.GetAppliedMigrationsAsync()).LastOrDefault();
        log.LogInformation("Міграції EF Core застосовано успішно. Остання: {Last}", last);
    }

    if (app.Environment.IsDevelopment() || app.Configuration.GetValue("Database:SeedDemoData", false))
        await DbSeeder.SeedAsync(db); // ТЕСТОВІ дані, лише якщо база порожня
}

// 5. Swagger: у Development завжди, у Production — якщо Swagger__Enabled=true
var swaggerEnabled = app.Environment.IsDevelopment() || app.Configuration.GetValue("Swagger:Enabled", false);
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS завершується на балансувальнику Render, тому UseHttpsRedirection не потрібен
app.UseCors("Frontend");

app.MapGet("/", () => Results.Ok(new
{
    service = "ItOpsMonitor.API",
    environment = app.Environment.EnvironmentName,
    database = provider.ToString(),
    health = "/health",
    swagger = swaggerEnabled ? "/swagger" : null,
    utc = DateTime.UtcNow
}));

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthResponseWriter.WriteAsync
});

app.MapControllers();

app.Run();
