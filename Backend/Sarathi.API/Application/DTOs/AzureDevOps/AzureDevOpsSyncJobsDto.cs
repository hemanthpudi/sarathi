namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class AzureDevOpsSyncJobsDto
{
    public DateTime GeneratedAtUtc { get; set; }

    public List<AzureDevOpsSyncJobSummaryDto> Items { get; set; } = new();
}