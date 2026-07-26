namespace Sarathi.API.Application.DTOs.Dashboard;

public class DashboardSprintStatisticsDto
{
    public int TotalSprints { get; set; }

    public int ActiveSprints { get; set; }

    public int CompletedSprints { get; set; }

    public decimal AverageCompletionRate { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<DashboardSprintStatisticItemDto> Sprints { get; set; } = new();
}