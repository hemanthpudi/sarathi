namespace Sarathi.API.Application.DTOs.Notifications;

public class NotificationDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; }

    public bool IsToast { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}