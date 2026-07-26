namespace Sarathi.API.Application.DTOs.ProjectManager;

/// <summary>
/// Comprehensive project context built from synchronized Azure DevOps data.
/// Used to provide rich context to AI analysis before calling Gemini.
/// </summary>
public class ProjectContextDto
{
    public ProjectSummaryDto Project { get; set; } = new();

    public List<WorkItemCategoryDto> Epics { get; set; } = new();

    public List<WorkItemCategoryDto> Features { get; set; } = new();

    public List<WorkItemCategoryDto> UserStories { get; set; } = new();

    public List<WorkItemCategoryDto> Tasks { get; set; } = new();

    public List<WorkItemCategoryDto> Bugs { get; set; } = new();

    public List<SprintSummaryDto> Sprints { get; set; } = new();

    public List<TeamMemberDto> Team { get; set; } = new();

    public KpiContextDto Kpis { get; set; } = new();

    public List<RiskSummaryDto> Risks { get; set; } = new();

    public List<PipelineSummaryDto> Pipelines { get; set; } = new();

    public List<RepositorySummaryDto> Repositories { get; set; } = new();

    public SynchronizationStatusDto SyncStatus { get; set; } = new();
}

public class ProjectSummaryDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Visibility { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}

public class WorkItemCategoryDto
{
    public int WorkItemId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string AssignedTo { get; set; } = string.Empty;

    public decimal? StoryPoints { get; set; }

    public int ProgressPercent { get; set; }

    public bool IsBlocked { get; set; }

    public DateTime LastUpdated { get; set; }

    public string? SprintName { get; set; }
}

public class SprintSummaryDto
{
    public int SprintId { get; set; }

    public string SprintName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int PlannedStoryPoints { get; set; }

    public int CompletedStoryPoints { get; set; }

    public int TotalWorkItems { get; set; }

    public int CompletedWorkItems { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal CompletionPercentage => TotalWorkItems > 0 ? (decimal)CompletedWorkItems / TotalWorkItems * 100 : 0;

    public decimal VelocityPercentage => PlannedStoryPoints > 0 ? (decimal)CompletedStoryPoints / PlannedStoryPoints * 100 : 0;
}

public class TeamMemberDto
{
    public string Name { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public decimal AllocationPercent { get; set; }

    public string DeliveryHealth { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty; // "ProjectManager", "WorkItemAssignment", etc.
}

public class KpiContextDto
{
    public decimal? SprintVelocity { get; set; }

    public decimal? CompletionRate { get; set; }

    public decimal? DefectDensity { get; set; }

    public decimal? BacklogHealth { get; set; }

    public decimal? ReleaseSuccessRate { get; set; }

    public DateTime? SnapshotDate { get; set; }
}

public class RiskSummaryDto
{
    public int Id { get; set; }

    public decimal RiskScore { get; set; }

    public string RiskLevel { get; set; } = string.Empty;

    public string? RiskSummary { get; set; }

    public DateTime GeneratedDate { get; set; }
}

public class PipelineSummaryDto
{
    public int BuildId { get; set; }

    public string DefinitionName { get; set; } = string.Empty;

    public string BuildNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Result { get; set; }

    public string SourceBranch { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime? FinishTime { get; set; }
}

public class RepositorySummaryDto
{
    public int RepositoryId { get; set; }

    public string RepositoryName { get; set; } = string.Empty;

    public string? DefaultBranch { get; set; }

    public long? Size { get; set; }

    public string? Url { get; set; }
}

public class SynchronizationStatusDto
{
    public DateTime? LastSyncUtc { get; set; }

    public string LastSyncStatus { get; set; } = string.Empty;

    public int? ItemsProcessedInLastSync { get; set; }

    public int? ItemsSucceededInLastSync { get; set; }

    public int? ItemsFailedInLastSync { get; set; }

    public string? LastSyncErrorMessage { get; set; }

    public int? LastSyncDurationSeconds { get; set; }
}
