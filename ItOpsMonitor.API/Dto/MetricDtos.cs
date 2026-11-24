using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ItOpsMonitor.Dto;

public class CreateMetricDto
{
    [JsonPropertyName("cpu_usage")]
    [Range(0, 100)] public double CpuUsage { get; set; }

    [JsonPropertyName("memory_usage")]
    [Range(0, 100)] public double MemoryUsage { get; set; }

    [JsonPropertyName("temperature")]
    [Range(-20, 120)] public decimal? Temperature { get; set; }

    [JsonPropertyName("humidity")]
    [Range(0, 100)] public decimal? Humidity { get; set; }
}

// MetricResponseDto з лабораторної №2: Temperature змінено з double на double?
public class MetricResponseDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("cpu_usage")] public double CpuUsage { get; set; }
    [JsonPropertyName("memory_usage")] public double MemoryUsage { get; set; }
    [JsonPropertyName("temperature")] public double? Temperature { get; set; }
    [JsonPropertyName("collected_at")] public DateTime CollectedAt { get; set; }
}
