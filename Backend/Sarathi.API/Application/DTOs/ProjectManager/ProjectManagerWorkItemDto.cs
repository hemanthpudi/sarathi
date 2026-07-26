namespace Sarathi.API.Application.DTOs.ProjectManager;

public class ProjectManagerWorkItemDto
{
    public int WorkItemId { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string AzureWorkItemId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string WorkItemType { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string AssignedToName { get; set; } = string.Empty;

    public decimal? StoryPoints { get; set; }

    public int ProgressPercent { get; set; }

    public bool IsBlocked { get; set; }

    public string SprintName { get; set; } = string.Empty;

    public DateTime LastUpdatedUtc { get; set; }
}