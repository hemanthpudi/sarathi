namespace Sarathi.API.Application.DTOs.Dashboard;

public class DashboardSprintStatisticItemDto
{
    public int SprintId { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string SprintName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int PlannedStoryPoints { get; set; }

    public int CompletedStoryPoints { get; set; }

    public int TotalWorkItems { get; set; }

    public int CompletedWorkItems { get; set; }

    public decimal CompletionRate { get; set; }
}