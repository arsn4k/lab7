using ItOpsMonitor.Domain.Entities;
using ItOpsMonitor.Domain.Exceptions;
using ItOpsMonitor.Domain.Repositories;
using ItOpsMonitor.Dto;
using Microsoft.AspNetCore.Mvc;

namespace ItOpsMonitor.API.Controllers;

[ApiController]
[Route("api/servers")]
public class ServersController : ControllerBase
{
    private readonly IServerRepository _repository;

    public ServersController(IServerRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServerResponseDto>>> GetAll()
    {
        var servers = await _repository.GetAllAsync();
        return Ok(servers.Select(s => s.ToDto()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServerResponseDto>> GetById(Guid id)
    {
        var server = await _repository.GetByIdAsync(id);
        return server is null
            ? NotFound(new { message = "Сервер не знайдено." })
            : Ok(server.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<ServerResponseDto>> Create(CreateServerDto dto)
    {
        // Зрозуміла відповідь клієнту; остаточний захист — UNIQUE-індекс у БД
        if (await _repository.GetByHostnameAsync(dto.Hostname) is not null)
            return Conflict(new
            {
                message = $"Сервер з hostname «{dto.Hostname}» уже зареєстровано."
            });

        var server = new Server
        {
            Hostname = dto.Hostname,
            IpAddress = dto.IpAddress,
            Environment = dto.Environment,
            CpuAlertThreshold = dto.CpuAlertThreshold
        };

        try
        {
            await _repository.AddAsync(server);
        }
        catch (DataIntegrityException ex)
        {
            return Conflict(new { message = ex.Message });
        }

        return CreatedAtAction(nameof(GetById), new { id = server.Id }, server.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServerResponseDto>> Update(Guid id, UpdateServerDto dto)
    {
        var server = await _repository.GetByIdAsync(id);
        if (server is null)
            return NotFound(new { message = "Сервер не знайдено." });

        server.Hostname = dto.Hostname;
        server.IpAddress = dto.IpAddress;
        server.Environment = dto.Environment;
        server.CpuAlertThreshold = dto.CpuAlertThreshold;
        server.CurrentStatus = dto.Status;

        try
        {
            await _repository.UpdateAsync(server);
        }
        catch (DataIntegrityException ex)
        {
            return Conflict(new { message = ex.Message });
        }

        return Ok(server.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _repository.DeleteAsync(id)
            ? NoContent()
            : NotFound(new { message = "Сервер не знайдено." });
    }

    [HttpGet("{id:guid}/metrics")]
    public async Task<ActionResult<IEnumerable<MetricResponseDto>>> GetMetrics(
        Guid id, [FromQuery] int take = 50)
    {
        if (await _repository.GetByIdAsync(id) is null)
            return NotFound(new { message = "Сервер не знайдено." });

        var metrics = await _repository.GetMetricsAsync(id, Math.Clamp(take, 1, 500));
        return Ok(metrics.Select(m => m.ToDto()));
    }

    [HttpPost("{id:guid}/metrics")]
    public async Task<ActionResult<MetricResponseDto>> AddMetric(
        Guid id, CreateMetricDto dto)
    {
        var sample = new MetricSample
        {
            ServerId = id,
            CpuUsage = dto.CpuUsage,
            MemoryUsage = dto.MemoryUsage,
            Temperature = dto.Temperature,
            Humidity = dto.Humidity
        };

        try
        {
            await _repository.AddMetricAsync(sample);
        }
        catch (DataIntegrityException ex)
        {
            return NotFound(new { message = ex.Message });   // сервера з таким Id немає
        }

        return Created($"api/servers/{id}/metrics", sample.ToDto());
    }
}
