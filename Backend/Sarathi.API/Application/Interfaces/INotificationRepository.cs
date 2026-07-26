using Sarathi.API.Application.DTOs.Notifications;
using Sarathi.API.Domain.Entities;

namespace Sarathi.API.Application.Interfaces;

public interface INotificationRepository
{
    Task<NotificationsOverviewDto> GetNotificationsAsync(Guid userId, int take, CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAsync(int notificationId, CancellationToken cancellationToken = default);

    Task<NotificationDto> CreateAsync(Notification notification, CancellationToken cancellationToken = default);

    Task<NotificationDto?> MarkAsReadAsync(Guid userId, int notificationId, CancellationToken cancellationToken = default);
}