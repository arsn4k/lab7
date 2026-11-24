using ItOpsMonitor.Domain.Entities;
using ItOpsMonitor.Domain.Enums;
using ItOpsMonitor.Domain.Exceptions;
using ItOpsMonitor.Infrastructure.Data;
using ItOpsMonitor.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ItOpsMonitor.Tests;

public class EfServerRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ItOpsDbContext _context;
    private readonly EfServerRepository _repo;

    public EfServerRepositoryTests()
    {
        _connection.Open();                        // БД живе, доки відкрите з'єднання
        var options = new DbContextOptionsBuilder<ItOpsDbContext>()
            .UseSqlite(_connection).Options;
        _context = new ItOpsDbContext(options);
        _context.Database.EnsureCreated();         // схема з моделі, разом із CHECK
        _repo = new EfServerRepository(_context);
    }

    private static Server NewServer(string host) => new()
        { Hostname = host, IpAddress = "10.0.0.1", Environment = "Staging" };

    [Fact]
    public async Task AddAsync_SetsDefaults_AndPersists()
    {
        await _repo.AddAsync(NewServer("web-01"));
        _context.ChangeTracker.Clear();

        var saved = (await _repo.GetByHostnameAsync("web-01"))!;
        Assert.Equal(ServerStatus.Maintenance, saved.CurrentStatus);
        Assert.Equal(90, saved.CpuAlertThreshold);
        Assert.Equal(DateTimeKind.Utc, saved.CreatedAt.Kind);
    }

    [Fact]
    public async Task AddAsync_DuplicateHostname_IgnoringCase_Throws()
    {
        await _repo.AddAsync(NewServer("web-01"));
        await Assert.ThrowsAsync<DataIntegrityException>(
            () => _repo.AddAsync(NewServer("WEB-01")));
    }

    [Fact]
    public async Task AddAsync_ThresholdOutOfRange_Throws()
    {
        var server = NewServer("bad-01");
        server.CpuAlertThreshold = 150;
        await Assert.ThrowsAsync<DataIntegrityException>(
            () => _repo.AddAsync(server));
    }

    [Fact]
    public async Task DeleteAsync_RemovesServer_AndItsMetrics()
    {
        var server = NewServer("db-01");
        await _repo.AddAsync(server);
        await _repo.AddMetricAsync(
            new MetricSample { ServerId = server.Id, CpuUsage = 10, MemoryUsage = 20 });

        Assert.True(await _repo.DeleteAsync(server.Id));
        Assert.Equal(0, await _context.MetricSamples.CountAsync());
        Assert.False(await _repo.DeleteAsync(server.Id));    // повторно — false
    }

    [Fact]
    public async Task GetMetricsAsync_ReturnsNewestFirst()
    {
        var server = NewServer("web-02");
        await _repo.AddAsync(server);
        var t = DateTime.UtcNow;
        await _repo.AddMetricAsync(new MetricSample
            { ServerId = server.Id, CpuUsage = 1, MemoryUsage = 1, CollectedAt = t.AddMinutes(-5) });
        await _repo.AddMetricAsync(new MetricSample
            { ServerId = server.Id, CpuUsage = 2, MemoryUsage = 2, CollectedAt = t });

        var list = (await _repo.GetMetricsAsync(server.Id)).ToList();
        Assert.Equal(2, list[0].CpuUsage);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
