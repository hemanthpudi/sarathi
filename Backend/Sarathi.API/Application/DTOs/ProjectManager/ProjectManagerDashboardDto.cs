namespace Sarathi.API.Application.DTOs.ProjectManager;

public class ProjectManagerDashboardDto
{
    public int AssignedProjects { get; set; }

    public int ActiveSprints { get; set; }

    public int OpenWorkItems { get; set; }

    public int BlockedWorkItems { get; set; }

    public decimal AverageCompletionRate { get; set; }

    public decimal AverageSprintVelocity { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<ProjectManagerChartPointDto> SprintVelocityTrend { get; set; } = new();

    public List<ProjectManagerChartPointDto> WorkItemsByState { get; set; } = new();

    public List<ProjectManagerAssignedProjectDto> SpotlightProjects { get; set; } = new();
}