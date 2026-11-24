using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ItOpsMonitor.API.Health;

/// <summary>Відповідь /health у форматі JSON (без повідомлень винятків — щоб не розкривати деталі).</summary>
public static class HealthResponseWriter
{
    public static Task WriteAsync(HttpContext http, HealthReport report)
    {
        var body = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            })
        };

        return http.Response.WriteAsJsonAsync(body);
    }
}
