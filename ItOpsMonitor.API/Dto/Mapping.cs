using ItOpsMonitor.Domain.Entities;

namespace ItOpsMonitor.Dto;

public static class Mapping
{
    public static ServerResponseDto ToDto(this Server s) => new()
    {
        Id = s.Id,
        Hostname = s.Hostname,
        IpAddress = s.IpAddress,
        Environment = s.Environment,
        CpuAlertThreshold = s.CpuAlertThreshold,
        Status = s.CurrentStatus,
        LastHeartbeat = s.LastHeartbeat
    };

    public static MetricResponseDto ToDto(this MetricSample m) => new()
    {
        Id = m.Id,
        CpuUsage = m.CpuUsage,
        MemoryUsage = m.MemoryUsage,
        Temperature = m.Temperature is null ? null : (double)m.Temperature,
        CollectedAt = m.CollectedAt
    };
}
