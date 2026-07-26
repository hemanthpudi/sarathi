namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class AzureDevOpsLogsDto
{
    public DateTime GeneratedAtUtc { get; set; }

    public List<AzureDevOpsLogEntryDto> Items { get; set; } = new();
}