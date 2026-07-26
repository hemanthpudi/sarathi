namespace Sarathi.API.Application.DTOs.Dashboard;

public class DashboardWorkItemSummariesDto
{
    public int TotalWorkItems { get; set; }

    public int OpenWorkItems { get; set; }

    public int InProgressWorkItems { get; set; }

    public int BlockedWorkItems { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<DashboardWorkItemSummaryItemDto> ByState { get; set; } = new();

    public List<DashboardWorkItemSummaryItemDto> ByType { get; set; } = new();
}