using ItOpsMonitor.Domain.Repositories;
using ItOpsMonitor.Infrastructure.Data;
using ItOpsMonitor.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ItOpsMonitor.Infrastructure;

public static class DependencyInjection
{
    private const string MigrationsAssembly = "ItOpsMonitor.Infrastructure";

    /// <summary>Реєструє контекст обраної СУБД і репозиторій.</summary>
    public static IServiceCollection AddItOpsDatabase(this IServiceCollection services,
        DatabaseProvider provider, string connectionString, bool isDevelopment)
    {
        void Common(DbContextOptionsBuilder options)
        {
            if (!isDevelopment) return;
            options.EnableSensitiveDataLogging(); // значення параметрів — лише під час розробки
            options.EnableDetailedErrors();
        }

        switch (provider)
        {
            case DatabaseProvider.Postgres:
                // Сервіс — ItOpsDbContext, реалізація — PostgresItOpsDbContext
                services.AddDbContext<ItOpsDbContext, PostgresItOpsDbContext>(options =>
                {
                    options.UseNpgsql(connectionString, npgsql =>
                    {
                        npgsql.MigrationsAssembly(MigrationsAssembly);
                        npgsql.EnableRetryOnFailure(5); // тимчасові збої мережі хмари
                    });
                    Common(options);
                });
                break;

            default:
                services.AddDbContext<ItOpsDbContext>(options =>
                {
                    options.UseSqlite(connectionString, s => s.MigrationsAssembly(MigrationsAssembly));
                    Common(options);
                });
                break;
        }

        services.AddScoped<IServerRepository, EfServerRepository>();
        return services;
    }
}
