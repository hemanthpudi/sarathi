namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class AzureDevOpsConnectionTestResultDto
{
    public bool IsConnected { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime TestedAtUtc { get; set; }

    public int ProjectCount { get; set; }
}