using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ItOpsMonitor.Infrastructure.Data;

// Фабрика потрібна лише інструменту dotnet ef: вона створює контекст PostgreSQL
// незалежно від того, який провайдер зараз увімкнено в appsettings.json.
// Рядок підключення береться зі змінної середовища — у файлах коду секретів немає.

public class PostgresDesignTimeFactory : IDesignTimeDbContextFactory<PostgresItOpsDbContext>
{
    public PostgresItOpsDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                 ?? "Host=localhost;Database=itops_design;Username=postgres";

        var options = new DbContextOptionsBuilder<PostgresItOpsDbContext>()
            .UseNpgsql(ConnectionStringResolver.Normalize(cs, DatabaseProvider.Postgres))
            .Options;

        return new PostgresItOpsDbContext(options);
    }
}
