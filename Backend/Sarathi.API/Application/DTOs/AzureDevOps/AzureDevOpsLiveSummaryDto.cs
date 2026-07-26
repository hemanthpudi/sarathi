namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class AzureDevOpsLiveSummaryDto
{
    public bool IsConfigured { get; set; }

    public string OrganizationUrl { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; }

    public int TotalProjects { get; set; }

    public int SuccessfulProjects { get; set; }

    public int WarningProjects { get; set; }

    public int FailedProjects { get; set; }

    public int TotalItems { get; set; }

    public double AverageSyncTimeSeconds { get; set; }

    public List<AzureDevOpsLiveProjectDto> Projects { get; set; } = new();

    public List<AzureDevOpsLiveTimelineItemDto> Timeline { get; set; } = new();

    public List<AzureDevOpsLiveMetricDto> Metrics { get; set; } = new();
}