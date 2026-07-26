namespace Sarathi.API.Domain.Entities;

public class Build
{
    public int BuildId { get; set; }

    public int AzureBuildId { get; set; }

    public int ProjectId { get; set; }

    public string DefinitionName { get; set; } = string.Empty;

    public string BuildNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Result { get; set; }

    public string SourceBranch { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime? FinishTime { get; set; }

    public Project Project { get; set; } = null!;
}