namespace Sarathi.API.Application.DTOs.Dashboard;

public class DashboardProjectStatisticItemDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Visibility { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; }

    public decimal? CompletionRate { get; set; }

    public decimal? SprintVelocity { get; set; }

    public decimal? DefectDensity { get; set; }

    public decimal? RiskScore { get; set; }

    public string? RiskLevel { get; set; }
}