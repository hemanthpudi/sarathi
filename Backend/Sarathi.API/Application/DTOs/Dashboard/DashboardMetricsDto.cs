namespace Sarathi.API.Application.DTOs.Dashboard;

public class DashboardMetricsDto
{
    public int TotalProjects { get; set; }

    public int ActiveSprints { get; set; }

    public int OpenWorkItems { get; set; }

    public int BlockedWorkItems { get; set; }

    public int RunningSyncJobs { get; set; }

    public DateTime GeneratedAtUtc { get; set; }
}