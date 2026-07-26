namespace Sarathi.API.Application.DTOs.ProjectManager;

public class ProjectManagerAssignedProjectDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Visibility { get; set; } = string.Empty;

    public string ProjectRole { get; set; } = string.Empty;

    public decimal AllocationPercent { get; set; }

    public string DeliveryHealth { get; set; } = string.Empty;

    public decimal? CompletionRate { get; set; }

    public decimal? SprintVelocity { get; set; }

    public int BlockedItems { get; set; }

    public DateTime LastUpdated { get; set; }
}