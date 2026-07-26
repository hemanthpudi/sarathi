namespace Sarathi.API.Application.DTOs.Dashboard;

public class DashboardProjectStatisticsDto
{
    public int TotalProjects { get; set; }

    public int PublicProjects { get; set; }

    public int PrivateProjects { get; set; }

    public int HighRiskProjects { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<DashboardProjectStatisticItemDto> Projects { get; set; } = new();
}