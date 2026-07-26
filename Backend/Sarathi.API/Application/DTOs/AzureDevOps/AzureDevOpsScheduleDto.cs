namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class AzureDevOpsScheduleDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SyncType { get; set; } = string.Empty;

    public string ScopeName { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public int IntervalMinutes { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime? LastRunUtc { get; set; }

    public DateTime? NextRunUtc { get; set; }
}