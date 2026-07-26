namespace Sarathi.API.Application.DTOs.ProjectManager;

public class ProjectManagerKpisDto
{
    public decimal AverageCompletionRate { get; set; }

    public decimal AverageSprintVelocity { get; set; }

    public decimal AverageDefectDensity { get; set; }

    public decimal AverageBacklogHealth { get; set; }

    public decimal AverageReleaseSuccessRate { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<ProjectManagerChartPointDto> CompletionTrend { get; set; } = new();

    public List<ProjectManagerChartPointDto> VelocityTrend { get; set; } = new();
}