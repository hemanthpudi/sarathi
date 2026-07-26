namespace Sarathi.API.Application.DTOs.ItAdmin;

public class ItAdminSynchronizationDto
{
    public int JobsRunning { get; set; }

    public int SuccessfulRuns24h { get; set; }

    public int FailedRuns24h { get; set; }

    public int QueuedSchedules { get; set; }

    public decimal AverageDurationSeconds { get; set; }

    public DateTime? LastSuccessfulSyncUtc { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<ItAdminChartPointDto> SyncTypeBreakdown { get; set; } = new();

    public List<ItAdminSyncJobDto> RecentJobs { get; set; } = new();
}