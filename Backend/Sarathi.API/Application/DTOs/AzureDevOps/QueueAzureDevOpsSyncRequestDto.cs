using System.ComponentModel.DataAnnotations;

namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class QueueAzureDevOpsSyncRequestDto
{
    [MaxLength(100)]
    public string SyncType { get; set; } = "Full";

    [MaxLength(200)]
    public string ScopeName { get; set; } = "All Projects";

    [MaxLength(50)]
    public string Source { get; set; } = "Manual";
}