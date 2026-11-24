namespace ItOpsMonitor.Domain.Entities;

public class MetricSample
{
    public long Id { get; set; }
    public Guid ServerId { get; set; }            // зовнішній ключ
    public double CpuUsage { get; set; }          // %
    public double MemoryUsage { get; set; }       // %
    public decimal? Temperature { get; set; }     // °C, null — датчика немає
    public decimal? Humidity { get; set; }        // %, null — датчика немає
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;

    public Server? Server { get; set; }           // навігація до власника
}
