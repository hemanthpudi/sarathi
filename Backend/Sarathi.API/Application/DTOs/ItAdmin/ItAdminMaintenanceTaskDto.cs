namespace Sarathi.API.Application.DTOs.ItAdmin;

public class ItAdminMaintenanceTaskDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string EnvironmentName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool IsRecurring { get; set; }

    public DateTime ScheduledStartUtc { get; set; }

    public DateTime? ScheduledEndUtc { get; set; }
}