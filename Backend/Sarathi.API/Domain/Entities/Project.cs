namespace Sarathi.API.Domain.Entities;

public class Project
{
    public int ProjectId { get; set; }

    public string AzureProjectId { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Visibility { get; set; } = "Private";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public ICollection<KpiSnapshot> KpiSnapshots { get; set; } = new List<KpiSnapshot>();

    public ICollection<ProjectManagerAssignment> ProjectManagerAssignments { get; set; } = new List<ProjectManagerAssignment>();

    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();

    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();

    public ICollection<Repository> Repositories { get; set; } = new List<Repository>();

    public ICollection<Build> Builds { get; set; } = new List<Build>();

    public ICollection<ProjectRiskAnalysis> RiskAnalyses { get; set; } = new List<ProjectRiskAnalysis>();
}
