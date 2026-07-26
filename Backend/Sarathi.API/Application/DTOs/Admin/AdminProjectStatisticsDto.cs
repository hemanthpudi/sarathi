namespace Sarathi.API.Application.DTOs.Admin;

public class AdminProjectStatisticsDto
{
    public int TotalProjects { get; set; }

    public int PublicProjects { get; set; }

    public int PrivateProjects { get; set; }

    public int HighRiskProjects { get; set; }

    public decimal AverageCompletionRate { get; set; }

    public decimal AverageSprintVelocity { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public IReadOnlyCollection<AdminProjectStatisticItemDto> Projects { get; set; } = Array.Empty<AdminProjectStatisticItemDto>();
}

public class AdminProjectStatisticItemDto
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

    public string? ProjectManagerName { get; set; }

    public string? ProjectManagerEmail { get; set; }
}
