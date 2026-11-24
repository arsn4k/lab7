using ItOpsMonitor.Domain.Entities;

namespace ItOpsMonitor.Domain.Repositories;

public interface IServerRepository
{
    Task<IEnumerable<Server>> GetAllAsync();
    Task<Server?> GetByIdAsync(Guid id);
    Task<Server?> GetByHostnameAsync(string hostname);
    Task AddAsync(Server server);
    Task UpdateAsync(Server server);
    Task<bool> DeleteAsync(Guid id);

    Task AddMetricAsync(MetricSample sample);
    Task<IEnumerable<MetricSample>> GetMetricsAsync(Guid serverId, int take = 50);
    Task<MetricSample?> GetLatestMetricAsync(Guid serverId);
}
