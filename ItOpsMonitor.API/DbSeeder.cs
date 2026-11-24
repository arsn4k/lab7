using ItOpsMonitor.Domain.Entities;
using ItOpsMonitor.Domain.Enums;
using ItOpsMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ItOpsMonitor.API;

/// <summary>ТЕСТОВІ дані для розробки. Це не показники реальних серверів.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ItOpsDbContext db)
    {
        if (await db.Servers.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var web = new Server { Hostname = "web-prod-01", IpAddress = "10.0.1.11",
            Environment = "Production", CurrentStatus = ServerStatus.Healthy,
            LastHeartbeat = now.AddSeconds(-20) };
        web.Metrics.Add(Sample(32.5, 58.1, 52.0m, 41.0m, now.AddMinutes(-10)));
        web.Metrics.Add(Sample(35.2, 58.9, 53.5m, 40.6m, now.AddMinutes(-5)));
        web.Metrics.Add(Sample(31.8, 59.4, 52.7m, 40.9m, now.AddMinutes(-1)));

        var db1 = new Server { Hostname = "db-prod-01", IpAddress = "10.0.1.21",
            Environment = "Production", CpuAlertThreshold = 85,
            CurrentStatus = ServerStatus.Degraded, LastHeartbeat = now.AddMinutes(-1) };
        db1.Metrics.Add(Sample(91.3, 77.0, 67.25m, 38.0m, now.AddMinutes(-2)));

        var cache = new Server { Hostname = "cache-stg-01", IpAddress = "10.0.2.15",
            Environment = "Staging", CurrentStatus = ServerStatus.Down };

        var build = new Server { Hostname = "build-dev-01", IpAddress = "10.0.3.8",
            Environment = "Development", CpuAlertThreshold = 95 };
        build.Metrics.Add(Sample(12.0, 30.5, null, null, now.AddMinutes(-20)));

        db.Servers.AddRange(web, db1, cache, build);
        await db.SaveChangesAsync();
    }

    private static MetricSample Sample(
        double cpu, double mem, decimal? t, decimal? h, DateTime at) => new()
    {
        CpuUsage = cpu, MemoryUsage = mem, Temperature = t, Humidity = h, CollectedAt = at
    };
}
