namespace Sarathi.API.Application.DTOs.Dashboard;

public class DashboardKpiCalculationsDto
{
    public decimal AverageCompletionRate { get; set; }

    public decimal AverageSprintVelocity { get; set; }

    public decimal AverageDefectDensity { get; set; }

    public decimal AverageBacklogHealth { get; set; }

    public decimal AverageReleaseSuccessRate { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<DashboardChartPointDto> CompletionTrend { get; set; } = new();

    public List<DashboardChartPointDto> VelocityTrend { get; set; } = new();
}