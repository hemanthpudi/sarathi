namespace Sarathi.API.Application.DTOs.Admin;

public class AdminProjectDetailsDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string AzureProjectId { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Visibility { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }

    public int TotalWorkItems { get; set; }

    public int CompletedWorkItems { get; set; }

    public int TotalSprints { get; set; }

    public int ActiveSprints { get; set; }

    public decimal? AverageCompletionRate { get; set; }

    public decimal? AverageSprintVelocity { get; set; }

    public decimal? RiskScore { get; set; }

    public string? RiskLevel { get; set; }

    public List<AdminProjectWorkItemDto> WorkItems { get; set; } = new();

    public List<AdminProjectSprintDto> Sprints { get; set; } = new();

    public List<AdminProjectRepositoryDto> Repositories { get; set; } = new();

    public List<AdminProjectBuildDto> Builds { get; set; } = new();

    public List<AdminProjectTeamMemberDto> TeamMembers { get; set; } = new();
}

public class AdminProjectWorkItemDto
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

    public DateTime LastUpdatedUtc { get; set; }

    public string? SprintName { get; set; }
}

public class AdminProjectSprintDto
{
    public int SprintId { get; set; }

    public string AzureIterationId { get; set; } = string.Empty;

    public string SprintName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int PlannedStoryPoints { get; set; }

    public int CompletedStoryPoints { get; set; }

    public int TotalWorkItems { get; set; }

    public int CompletedWorkItems { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class AdminProjectTeamMemberDto
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string ProjectRole { get; set; } = string.Empty;

    public decimal AllocationPercent { get; set; }
}

public class AdminProjectRepositoryDto
{
    public int RepositoryId { get; set; }

    public string AzureRepoId { get; set; } = string.Empty;

    public string RepositoryName { get; set; } = string.Empty;

    public string? DefaultBranch { get; set; }

    public long? Size { get; set; }

    public string? Url { get; set; }
}

public class AdminProjectBuildDto
{
    public int BuildId { get; set; }

    public int AzureBuildId { get; set; }

    public string DefinitionName { get; set; } = string.Empty;

    public string BuildNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Result { get; set; }

    public string SourceBranch { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime? FinishTime { get; set; }
}
