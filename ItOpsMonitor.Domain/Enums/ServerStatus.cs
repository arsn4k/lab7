namespace ItOpsMonitor.Domain.Enums;

/// <summary>Стан сервера. Числові значення стабільні: їх не можна змінювати.</summary>
public enum ServerStatus
{
    Healthy = 0,
    Degraded = 1,
    Down = 2,
    Maintenance = 3
}
