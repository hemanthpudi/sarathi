namespace Sarathi.API.Application.DTOs.Admin;

public class AdminConfigurationDto
{
    public int KpiRefreshIntervalMinutes { get; set; }

    public decimal SprintVelocityTarget { get; set; }

    public decimal CompletionRateTarget { get; set; }

    public decimal DefectDensityThreshold { get; set; }

    public decimal BacklogHealthThreshold { get; set; }

    public decimal ReleaseSuccessRateTarget { get; set; }

    public string AzureDevOpsOrganizationUrl { get; set; } = string.Empty;

    public string AzureDevOpsProjectFilter { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }
}
