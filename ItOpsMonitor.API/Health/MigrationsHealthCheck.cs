using ItOpsMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ItOpsMonitor.API.Health;

/// <summary>Healthy, якщо всі міграції EF Core застосовано, тобто схема БД актуальна.</summary>
public class MigrationsHealthCheck : IHealthCheck
{
    private readonly ItOpsDbContext _db;

    public MigrationsHealthCheck(ItOpsDbContext db)
    {
        _db = db;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var applied = (await _db.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();
        var pending = (await _db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        var data = new Dictionary<string, object>
        {
            ["applied"] = applied.Count,
            ["last"] = applied.LastOrDefault() ?? "—",
            ["pending"] = pending.Count
        };

        return pending.Count == 0
            ? HealthCheckResult.Healthy("Схема БД актуальна", data: data)
            : HealthCheckResult.Unhealthy($"Не застосовано міграцій: {pending.Count}", data: data);
    }
}
