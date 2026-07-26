using Sarathi.API.Application.DTOs.Notifications;

namespace Sarathi.API.Application.Interfaces;

public interface INotificationRealtimeService
{
    Task BroadcastAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default);
}