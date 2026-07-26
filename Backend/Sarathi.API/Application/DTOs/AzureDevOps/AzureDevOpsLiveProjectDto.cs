namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class AzureDevOpsLiveProjectDto
{
    public string ProjectName { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime? LastSyncUtc { get; set; }

    public int Items { get; set; }

    public double? DurationSeconds { get; set; }

    public int Errors { get; set; }

    public DateTime? LatestActivityUtc { get; set; }

    public string LatestActivityMessage { get; set; } = string.Empty;
}