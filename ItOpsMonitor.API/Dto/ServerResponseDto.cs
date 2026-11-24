using System.Text.Json.Serialization;
using ItOpsMonitor.Domain.Enums;

namespace ItOpsMonitor.Dto;

public class ServerResponseDto
{
    [JsonPropertyName("server_id")] public Guid Id { get; set; }
    [JsonPropertyName("hostname")] public string Hostname { get; set; } = string.Empty;
    [JsonPropertyName("ip_address")] public string IpAddress { get; set; } = string.Empty;
    [JsonPropertyName("environment")] public string Environment { get; set; } = string.Empty;
    [JsonPropertyName("cpu_alert_threshold")] public int CpuAlertThreshold { get; set; }
    [JsonPropertyName("status")] public ServerStatus Status { get; set; }
    [JsonPropertyName("last_heartbeat")] public DateTime? LastHeartbeat { get; set; }
}
