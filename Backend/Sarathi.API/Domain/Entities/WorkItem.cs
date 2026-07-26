namespace Sarathi.API.Domain.Entities;

public class WorkItem
{
    public int WorkItemId { get; set; }

    public int ProjectId { get; set; }

    public int? SprintId { get; set; }

    public string AzureWorkItemId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string WorkItemType { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string AssignedToName { get; set; } = string.Empty;

    public decimal? StoryPoints { get; set; }

    public int ProgressPercent { get; set; }

    public bool IsBlocked { get; set; }

    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }

    public Sprint? Sprint { get; set; }
}