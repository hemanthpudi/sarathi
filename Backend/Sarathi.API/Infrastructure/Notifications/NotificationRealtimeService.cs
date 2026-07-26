using Microsoft.AspNetCore.SignalR;
using Sarathi.API.Application.DTOs.Notifications;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Infrastructure.Notifications;

public class NotificationRealtimeService : INotificationRealtimeService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationRealtimeService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task BroadcastAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.Group(NotificationHub.GetUserGroup(userId)).SendAsync("notificationReceived", notification, cancellationToken);
    }
}