namespace Sarathi.API.Domain.Entities;

public class Repository
{
    public int RepositoryId { get; set; }

    public string AzureRepoId { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public string RepositoryName { get; set; } = string.Empty;

    public string? DefaultBranch { get; set; }

    public long? Size { get; set; }

    public string? Url { get; set; }

    public Project Project { get; set; } = null!;
}