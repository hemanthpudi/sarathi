namespace Sarathi.API.Application.DTOs.ProjectManager;

/// <summary>
/// A project-scoped sprint dashboard.  Unlike the existing project-manager
/// summaries, this response never combines data from multiple assignments.
/// </summary>
public class ProjectSprintGovernanceDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string DeliveryHealth { get; set; } = "On Track";

    public string SprintName { get; set; } = "No active sprint";

    public int SprintId { get; set; }

    public decimal SprintHealth { get; set; }

    public decimal CapacityUsedPercent { get; set; }

    public decimal CompletedStoryPoints { get; set; }

    public decimal PlannedStoryPoints { get; set; }

    public int CompletedItems { get; set; }

    public int PlannedItems { get; set; }

    public int DelayedItems { get; set; }

    public DateTime? SprintStartDate { get; set; }

    public DateTime? SprintEndDate { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<ProjectSprintBoardColumnDto> Board { get; set; } = new();
}

public class ProjectSprintBoardColumnDto
{
    public string Name { get; set; } = string.Empty;

    public List<ProjectSprintBoardItemDto> Items { get; set; } = new();
}

public class ProjectSprintBoardItemDto
{
    public int WorkItemId { get; set; }

    public string AzureWorkItemId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string WorkItemType { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string AssignedToName { get; set; } = string.Empty;

    public decimal? StoryPoints { get; set; }

    public int ProgressPercent { get; set; }

    public bool IsBlocked { get; set; }

    public bool IsDelayed { get; set; }

    public DateTime LastUpdatedUtc { get; set; }
}
