namespace Sarathi.API.Application.Interfaces;

public interface IAzureDevOpsClient
{
    Task<IReadOnlyList<AzureDevOpsProjectData>> GetProjectsAsync(string organizationUrl, string personalAccessToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AzureDevOpsIterationData>> GetSprintsAsync(string organizationUrl, string projectId, string projectName, string personalAccessToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AzureDevOpsWorkItemData>> GetWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AzureDevOpsRepositoryData>> GetRepositoriesAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AzureDevOpsBuildData>> GetBuildsAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AzureDevOpsReleaseData>> GetReleasesAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken = default);
}

public sealed class AzureDevOpsTeamData
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}

public sealed class AzureDevOpsProjectData
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Visibility { get; init; } = "Private";

    public string? Description { get; init; }
}

public sealed class AzureDevOpsIterationData
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public string Status { get; init; } = "Planned";
}

public sealed class AzureDevOpsWorkItemData
{
    public string Id { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string WorkItemType { get; init; } = string.Empty;

    public string State { get; init; } = string.Empty;

    public string Priority { get; init; } = string.Empty;

    public string AssignedToName { get; init; } = string.Empty;

    public decimal? StoryPoints { get; init; }

    public decimal? Effort { get; init; }

    public decimal? RemainingWork { get; init; }

    public bool IsBlocked { get; init; }

    public string SprintName { get; init; } = string.Empty;

    public DateTime? CreatedDateUtc { get; init; }

    public DateTime LastUpdatedUtc { get; init; }

    public DateTime? ClosedDateUtc { get; init; }
}

public sealed class AzureDevOpsRepositoryData
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? DefaultBranch { get; init; }

    public long? Size { get; init; }

    public string? Url { get; init; }
}

public sealed class AzureDevOpsBuildData
{
    public int Id { get; init; }

    public string BuildNumber { get; init; } = string.Empty;

    public string DefinitionName { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Result { get; init; } = string.Empty;

    public string SourceBranch { get; init; } = string.Empty;

    public DateTime? StartTimeUtc { get; init; }

    public DateTime? FinishTimeUtc { get; init; }
}

public sealed class AzureDevOpsReleaseData
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public DateTime? CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}