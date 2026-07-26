using System.ComponentModel.DataAnnotations;

namespace Sarathi.API.Application.DTOs.ProjectManager;

public class ProjectManagerAiAnalysisDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal CompletionRate { get; set; }
    public decimal SprintVelocity { get; set; }
    public decimal RiskScore { get; set; }
    public int BlockedItems { get; set; }
    public int ActiveSprints { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string RiskAnalysis { get; set; } = string.Empty;
    public string Report { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
    public string Provider { get; set; } = "Metrics fallback";
    public DateTime GeneratedAtUtc { get; set; }
}

public class ProjectManagerAiAnalysisRequestDto
{
    [MaxLength(2000)]
    public string? Question { get; set; }
}
