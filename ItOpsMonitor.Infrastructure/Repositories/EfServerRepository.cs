using ItOpsMonitor.Domain.Entities;
using ItOpsMonitor.Domain.Exceptions;
using ItOpsMonitor.Domain.Repositories;
using ItOpsMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ItOpsMonitor.Infrastructure.Repositories;

// Порівняно з лабораторною №6 змінено лише SaveAsync: тепер він розпізнає
// помилки обмежень не тільки SQLite, а й PostgreSQL.
public class EfServerRepository : IServerRepository
{
    private readonly ItOpsDbContext _context;

    public EfServerRepository(ItOpsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Server>> GetAllAsync()
        => await _context.Servers.AsNoTracking().OrderBy(s => s.Hostname).ToListAsync();

    public async Task<Server?> GetByIdAsync(Guid id)
        => await _context.Servers.FindAsync(id);

    public async Task<Server?> GetByHostnameAsync(string hostname)
        => await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Hostname == hostname);

    public async Task AddAsync(Server server)
    {
        await _context.Servers.AddAsync(server);
        await SaveAsync();
    }

    public async Task UpdateAsync(Server server)
    {
        _context.Servers.Update(server);
        await SaveAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var server = await GetByIdAsync(id);
        if (server is null)
            return false;

        _context.Servers.Remove(server); // метрики видалить каскад у БД
        await SaveAsync();
        return true;
    }

    public async Task AddMetricAsync(MetricSample sample)
    {
        await _context.MetricSamples.AddAsync(sample);
        await SaveAsync();
    }

    public async Task<IEnumerable<MetricSample>> GetMetricsAsync(Guid serverId, int take = 50)
        => await _context.MetricSamples.AsNoTracking()
            .Where(m => m.ServerId == serverId)
            .OrderByDescending(m => m.CollectedAt)
            .Take(take)
            .ToListAsync();

    public async Task<MetricSample?> GetLatestMetricAsync(Guid serverId)
        => await _context.MetricSamples.AsNoTracking()
            .Where(m => m.ServerId == serverId)
            .OrderByDescending(m => m.CollectedAt)
            .FirstOrDefaultAsync();

    // Єдина точка збереження: переклад помилок будь-якої СУБД у доменний виняток
    private async Task SaveAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ConstraintViolation.TryDescribe(ex.InnerException, out var message))
        {
            _context.ChangeTracker.Clear(); // не залишати «зламаний» запис у контексті
            throw new DataIntegrityException(message, ex);
        }
    }
}
