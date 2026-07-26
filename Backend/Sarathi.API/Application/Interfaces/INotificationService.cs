using Sarathi.API.Application.DTOs.Notifications;

namespace Sarathi.API.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationsOverviewDto> GetNotificationsAsync(Guid userId, int take, CancellationToken cancellationToken = default);

    Task<NotificationDto> CreateAsync(CreateNotificationRequestDto request, Guid fallbackUserId, CancellationToken cancellationToken = default);

    Task<NotificationDto?> MarkAsReadAsync(Guid userId, int notificationId, CancellationToken cancellationToken = default);
}