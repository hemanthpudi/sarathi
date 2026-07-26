namespace Sarathi.API.Domain.Entities;

public class Sprint
{
    public int SprintId { get; set; }

    public int ProjectId { get; set; }

    public string AzureIterationId { get; set; } = string.Empty;

    public string SprintName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int PlannedStoryPoints { get; set; }

    public int CompletedStoryPoints { get; set; }

    public int TotalWorkItems { get; set; }

    public int CompletedWorkItems { get; set; }

    public string Status { get; set; } = "Planned";

    public Project? Project { get; set; }

    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}