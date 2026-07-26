namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class AzureDevOpsIntegrationConfigurationDto
{
    public string OrganizationUrl { get; set; } = string.Empty;

    public string ProjectFilter { get; set; } = string.Empty;

    public bool SyncEnabled { get; set; }

    public bool HasPersonalAccessTokenConfigured { get; set; }

    public DateTime? PersonalAccessTokenUpdatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}