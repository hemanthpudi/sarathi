namespace Sarathi.API.Application.DTOs.ItAdmin;

public class ItAdminMaintenanceDto
{
    public int ActiveTasks { get; set; }

    public int ScheduledThisWeek { get; set; }

    public int OverdueTasks { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<ItAdminMaintenanceTaskDto> Items { get; set; } = new();
}