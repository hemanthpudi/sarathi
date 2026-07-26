namespace Sarathi.API.Domain.Entities;

public class AdminConfiguration
{
    public int Id { get; set; }

    public int KpiRefreshIntervalMinutes { get; set; } = 60;

    public decimal SprintVelocityTarget { get; set; } = 45;

    public decimal CompletionRateTarget { get; set; } = 85;

    public decimal DefectDensityThreshold { get; set; } = 1.5m;

    public decimal BacklogHealthThreshold { get; set; } = 75;

    public decimal ReleaseSuccessRateTarget { get; set; } = 90;

    public string AzureDevOpsOrganizationUrl { get; set; } = string.Empty;

    public string AzureDevOpsProjectFilter { get; set; } = string.Empty;

    public bool AzureDevOpsSyncEnabled { get; set; }

    public string AzureDevOpsPatCipherText { get; set; } = string.Empty;

    public DateTime? AzureDevOpsPatUpdatedAtUtc { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? UpdatedByUserId { get; set; }
}
