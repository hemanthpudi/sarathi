using System.ComponentModel.DataAnnotations;

namespace Sarathi.API.Application.DTOs.AzureDevOps;

public class TestAzureDevOpsConnectionRequestDto
{
    [MaxLength(500)]
    public string OrganizationUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? PersonalAccessToken { get; set; }

    [MaxLength(256)]
    public string? ProjectFilter { get; set; }
}