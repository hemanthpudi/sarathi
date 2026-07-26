namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class AzureDevOpsSyncQueueResponseDto
{
    public int JobId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime QueuedAtUtc { get; set; }
}