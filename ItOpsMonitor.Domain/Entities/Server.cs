using ItOpsMonitor.Domain.Enums;

namespace ItOpsMonitor.Domain.Entities;

public class Server
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public int CpuAlertThreshold { get; set; } = 90;
    public ServerStatus CurrentStatus { get; set; } = ServerStatus.Maintenance;
    public DateTime? LastHeartbeat { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навігаційна властивість: усі вимірювання цього сервера (зв'язок 1 : N)
    public List<MetricSample> Metrics { get; set; } = new();
}
