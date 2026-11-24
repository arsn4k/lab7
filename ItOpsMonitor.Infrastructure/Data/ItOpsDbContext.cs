using ItOpsMonitor.Domain.Entities;
using ItOpsMonitor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItOpsMonitor.Infrastructure.Data;

/// <summary>Базовий контекст. Без змін використовується для SQLite (міграції лабораторної №6).</summary>
public class ItOpsDbContext : DbContext
{
    public ItOpsDbContext(DbContextOptions<ItOpsDbContext> options) : base(options)
    {
    }

    // Конструктор для похідного контексту PostgreSQL
    protected ItOpsDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Server> Servers => Set<Server>();
    public DbSet<MetricSample> MetricSamples => Set<MetricSample>();

    // ---- Точки розширення: діалект SQL конкретної СУБД ----
    // У SQLite decimal зберігається як TEXT, тому для порівняння потрібен CAST
    protected virtual string DecimalColumn(string column) => $"CAST(\"{column}\" AS REAL)";

    protected virtual void ConfigureHostnameCollation(PropertyBuilder<string> hostname)
        => hostname.UseCollation("NOCASE");

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        builder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Server>(entity =>
        {
            entity.ToTable("Servers", t =>
            {
                t.HasCheckConstraint("CK_Servers_Hostname",
                    "LENGTH(\"Hostname\") BETWEEN 1 AND 100");
                t.HasCheckConstraint("CK_Servers_CpuAlertThreshold",
                    "\"CpuAlertThreshold\" BETWEEN 1 AND 100");
                t.HasCheckConstraint("CK_Servers_Environment",
                    "\"Environment\" IN ('Production', 'Staging', 'Development')");
                t.HasCheckConstraint("CK_Servers_CurrentStatus",
                    "\"CurrentStatus\" IN ('Healthy', 'Degraded', 'Down', 'Maintenance')");
            });

            entity.HasKey(s => s.Id);

            var hostname = entity.Property(s => s.Hostname).IsRequired().HasMaxLength(100);
            ConfigureHostnameCollation(hostname); // web-01 і WEB-01 — той самий сервер
            entity.HasIndex(s => s.Hostname).IsUnique();

            entity.Property(s => s.IpAddress).IsRequired().HasMaxLength(45);
            entity.Property(s => s.Environment).IsRequired().HasMaxLength(30);

            entity.Property(s => s.CpuAlertThreshold)
                .HasDefaultValue(90)
                .HasSentinel(90);

            entity.Property(s => s.CurrentStatus)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ServerStatus.Maintenance)
                .HasSentinel(ServerStatus.Maintenance);

            entity.HasMany(s => s.Metrics)
                .WithOne(m => m.Server)
                .HasForeignKey(m => m.ServerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MetricSample>(entity =>
        {
            entity.ToTable("MetricSamples", t =>
            {
                t.HasCheckConstraint("CK_MetricSamples_CpuUsage",
                    "\"CpuUsage\" BETWEEN 0 AND 100");
                t.HasCheckConstraint("CK_MetricSamples_MemoryUsage",
                    "\"MemoryUsage\" BETWEEN 0 AND 100");
                t.HasCheckConstraint("CK_MetricSamples_Temperature",
                    $"{DecimalColumn("Temperature")} BETWEEN -20 AND 120");
                t.HasCheckConstraint("CK_MetricSamples_Humidity",
                    $"{DecimalColumn("Humidity")} BETWEEN 0 AND 100");
            });

            entity.HasKey(m => m.Id);
            entity.Property(m => m.Temperature).HasPrecision(5, 2);
            entity.Property(m => m.Humidity).HasPrecision(5, 2);
            entity.HasIndex(m => new { m.ServerId, m.CollectedAt });
        });
    }
}
