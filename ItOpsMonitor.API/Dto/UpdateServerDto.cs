using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ItOpsMonitor.Domain.Enums;

namespace ItOpsMonitor.Dto;

public class UpdateServerDto : CreateServerDto
{
    [JsonPropertyName("status")]
    [EnumDataType(typeof(ServerStatus), ErrorMessage = "Невідомий статус сервера.")]
    public ServerStatus Status { get; set; } = ServerStatus.Maintenance;
}
