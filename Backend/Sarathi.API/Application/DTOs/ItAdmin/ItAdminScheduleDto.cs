namespace Sarathi.API.Application.DTOs.ItAdmin;

public class ItAdminScheduleDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string JobType { get; set; } = string.Empty;

    public string ScopeName { get; set; } = string.Empty;

    public string CronExpression { get; set; } = string.Empty;

    public string TimeZoneId { get; set; } = string.Empty;

    public string RunWindow { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public int FailureCount { get; set; }

    public DateTime? LastRunUtc { get; set; }

    public DateTime? NextRunUtc { get; set; }
}