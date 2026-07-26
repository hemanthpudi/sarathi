namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class AzureDevOpsLiveTimelineItemDto
{
    public string ProjectName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Items { get; set; }

    public DateTime? TimeUtc { get; set; }

    public string Message { get; set; } = string.Empty;
}