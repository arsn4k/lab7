using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItOpsMonitor.Infrastructure.Data;

/// <summary>Контекст для PostgreSQL (Render). Власні міграції: Migrations/Postgres.</summary>
public class PostgresItOpsDbContext : ItOpsDbContext
{
    private const string CaseInsensitive = "ci_hostname";

    public PostgresItOpsDbContext(DbContextOptions<PostgresItOpsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Недетермінована ICU-колація: 'web-01' = 'WEB-01' (PostgreSQL 12+)
        modelBuilder.HasCollation(CaseInsensitive, locale: "und-u-ks-level2",
            provider: "icu", deterministic: false);

        base.OnModelCreating(modelBuilder);
    }

    // numeric(5,2) — справжній числовий тип, CAST не потрібен
    protected override string DecimalColumn(string column) => $"\"{column}\"";

    protected override void ConfigureHostnameCollation(PropertyBuilder<string> hostname)
        => hostname.UseCollation(CaseInsensitive);
}
