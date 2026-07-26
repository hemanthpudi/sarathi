using Sarathi.API.Application.DTOs.Notifications;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Domain.Entities;

namespace Sarathi.API.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationRealtimeService _notificationRealtimeService;

    public NotificationService(INotificationRepository notificationRepository, INotificationRealtimeService notificationRealtimeService)
    {
        _notificationRepository = notificationRepository;
        _notificationRealtimeService = notificationRealtimeService;
    }

    public Task<NotificationsOverviewDto> GetNotificationsAsync(Guid userId, int take, CancellationToken cancellationToken = default)
    {
        var normalizedTake = take <= 0 ? 25 : Math.Min(take, 100);
        return _notificationRepository.GetNotificationsAsync(userId, normalizedTake, cancellationToken);
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationRequestDto request, Guid fallbackUserId, CancellationToken cancellationToken = default)
    {
        var userId = request.UserId ?? fallbackUserId;
        var notification = new Notification
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
            Severity = string.IsNullOrWhiteSpace(request.Severity) ? "Info" : request.Severity.Trim(),
            ActionUrl = string.IsNullOrWhiteSpace(request.ActionUrl) ? null : request.ActionUrl.Trim(),
            IsToast = request.IsToast,
            CreatedAtUtc = DateTime.UtcNow,
        };

        var created = await _notificationRepository.CreateAsync(notification, cancellationToken);
        await _notificationRealtimeService.BroadcastAsync(userId, created, cancellationToken);
        return created;
    }

    public Task<NotificationDto?> MarkAsReadAsync(Guid userId, int notificationId, CancellationToken cancellationToken = default)
    {
        return _notificationRepository.MarkAsReadAsync(userId, notificationId, cancellationToken);
    }
}