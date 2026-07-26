using System.ComponentModel.DataAnnotations;

namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class UpdateAzureDevOpsIntegrationConfigurationRequestDto
{
    [MaxLength(500)]
    public string OrganizationUrl { get; set; } = string.Empty;

    [MaxLength(256)]
    public string ProjectFilter { get; set; } = string.Empty;

    public bool SyncEnabled { get; set; }

    [MaxLength(200)]
    public string? PersonalAccessToken { get; set; }
}