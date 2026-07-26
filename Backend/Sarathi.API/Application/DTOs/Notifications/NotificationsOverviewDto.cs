namespace Sarathi.API.Application.DTOs.Notifications;

public class NotificationsOverviewDto
{
    public int UnreadCount { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<NotificationDto> Items { get; set; } = new();
}