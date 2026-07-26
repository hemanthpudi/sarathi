using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.DTOs.Notifications;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Domain.Entities;
using Sarathi.API.Persistence;
using System.Data.Common;

namespace Sarathi.API.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationsOverviewDto> GetNotificationsAsync(Guid userId, int take, CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _dbContext.Notifications
                .AsNoTracking()
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.CreatedAtUtc)
                .Take(take)
                .Select(item => ToDto(item))
                .ToListAsync(cancellationToken);

            var unreadCount = await _dbContext.Notifications
                .AsNoTracking()
                .CountAsync(item => item.UserId == userId && !item.IsRead, cancellationToken);

            return new NotificationsOverviewDto
            {
                UnreadCount = unreadCount,
                GeneratedAtUtc = DateTime.UtcNow,
                Items = items,
            };
        }
        catch (DbException)
        {
            return new NotificationsOverviewDto
            {
                UnreadCount = 0,
                GeneratedAtUtc = DateTime.UtcNow,
                Items = [],
            };
        }
    }

    public Task<Notification?> GetByIdAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications.FirstOrDefaultAsync(item => item.Id == notificationId, cancellationToken);
    }

    public async Task<NotificationDto> CreateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbException)
        {
        }

        return ToDto(notification);
    }

    public async Task<NotificationDto?> MarkAsReadAsync(Guid userId, int notificationId, CancellationToken cancellationToken = default)
    {
        Notification? notification;

        try
        {
            notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(item => item.Id == notificationId && item.UserId == userId, cancellationToken);
        }
        catch (DbException)
        {
            return null;
        }

        if (notification is null)
        {
            return null;
        }

        try
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbException)
        {
            return null;
        }

        return ToDto(notification);
    }

    private static NotificationDto ToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Category = notification.Category,
            Severity = notification.Severity,
            ActionUrl = notification.ActionUrl,
            IsRead = notification.IsRead,
            IsToast = notification.IsToast,
            CreatedAtUtc = notification.CreatedAtUtc,
        };
    }
}