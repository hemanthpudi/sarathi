namespace Sarathi.API.Domain.Entities;

public class ProjectRiskAnalysis
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public decimal RiskScore { get; set; }

    public string RiskLevel { get; set; } = string.Empty;

    public string? RiskSummary { get; set; }

    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
}
