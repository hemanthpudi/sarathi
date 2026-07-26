namespace Sarathi.API.Domain.Entities;

public class AutomationSchedule
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string JobType { get; set; } = string.Empty;

    public string ScopeName { get; set; } = string.Empty;

    public string Source { get; set; } = "Azure DevOps";

    public string CronExpression { get; set; } = string.Empty;

    public int IntervalMinutes { get; set; } = 60;

    public string TimeZoneId { get; set; } = "UTC";

    public string RunWindow { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public int FailureCount { get; set; }

    public DateTime? LastRunUtc { get; set; }

    public DateTime? NextRunUtc { get; set; }
}