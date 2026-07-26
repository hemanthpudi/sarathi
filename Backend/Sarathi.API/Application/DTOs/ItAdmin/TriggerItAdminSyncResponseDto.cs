namespace Sarathi.API.Application.DTOs.ItAdmin;

public class TriggerItAdminSyncResponseDto
{
    public int JobId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime QueuedAtUtc { get; set; }
}