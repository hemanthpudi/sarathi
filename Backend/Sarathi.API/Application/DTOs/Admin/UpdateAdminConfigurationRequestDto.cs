using System.ComponentModel.DataAnnotations;

namespace Sarathi.API.Application.DTOs.Admin;

public class UpdateAdminConfigurationRequestDto
{
    [Range(5, 1440)]
    public int KpiRefreshIntervalMinutes { get; set; }

    [Range(0, 1000)]
    public decimal SprintVelocityTarget { get; set; }

    [Range(0, 100)]
    public decimal CompletionRateTarget { get; set; }

    [Range(0, 100)]
    public decimal DefectDensityThreshold { get; set; }

    [Range(0, 100)]
    public decimal BacklogHealthThreshold { get; set; }

    [Range(0, 100)]
    public decimal ReleaseSuccessRateTarget { get; set; }

    [MaxLength(500)]
    public string AzureDevOpsOrganizationUrl { get; set; } = string.Empty;

    [MaxLength(256)]
    public string AzureDevOpsProjectFilter { get; set; } = string.Empty;
}
