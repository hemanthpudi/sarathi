using System.ComponentModel.DataAnnotations;

namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class UpsertAzureDevOpsScheduleRequestDto
{
    public int? Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SyncType { get; set; } = "Full";

    [MaxLength(200)]
    public string ScopeName { get; set; } = "All Projects";

    [Range(5, 1440)]
    public int IntervalMinutes { get; set; } = 60;

    public bool IsEnabled { get; set; } = true;
}