using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ItOpsMonitor.Dto;

public class CreateServerDto
{
    [JsonPropertyName("hostname")]
    [Required(ErrorMessage = "Hostname обов'язковий.")]
    [StringLength(100, ErrorMessage = "Hostname — не більше 100 символів.")]
    public string Hostname { get; set; } = string.Empty;

    [JsonPropertyName("ip_address")]
    [Required(ErrorMessage = "IP-адреса обов'язкова.")]
    [IpAddressValue(ErrorMessage = "Некоректна IP-адреса.")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("environment")]
    [RegularExpression("^(Production|Staging|Development)$",
        ErrorMessage = "Середовище: Production, Staging або Development.")]
    public string Environment { get; set; } = string.Empty;

    [JsonPropertyName("cpu_alert_threshold")]
    [Range(1, 100, ErrorMessage = "Поріг CPU має бути від 1 до 100 %.")]
    public int CpuAlertThreshold { get; set; } = 90;
}

public class IpAddressValueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is string s && System.Net.IPAddress.TryParse(s, out _);
}
