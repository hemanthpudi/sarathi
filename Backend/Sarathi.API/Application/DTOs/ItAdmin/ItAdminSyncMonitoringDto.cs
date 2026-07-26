namespace Sarathi.API.Application.DTOs.ItAdmin;

public class ItAdminSyncMonitoringDto
{
    public int TotalJobsToday { get; set; }

    public int RunningJobs { get; set; }

    public int FailedJobs { get; set; }

    public int WarningLogs { get; set; }

    public int OverdueSchedules { get; set; }

    public int ActiveMaintenanceWindows { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<ItAdminChartPointDto> DurationTrend { get; set; } = new();

    public List<ItAdminChartPointDto> StatusBreakdown { get; set; } = new();

    public List<ItAdminSyncJobDto> RecentJobs { get; set; } = new();
}