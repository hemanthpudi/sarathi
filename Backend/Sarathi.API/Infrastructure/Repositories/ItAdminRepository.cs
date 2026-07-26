using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.DTOs.ItAdmin;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Domain.Enums;
using Sarathi.API.Domain.Entities;
using Sarathi.API.Persistence;

namespace Sarathi.API.Infrastructure.Repositories;

public class ItAdminRepository : IItAdminRepository
{
    private readonly AppDbContext _dbContext;

    public ItAdminRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ItAdminSynchronizationDto> GetSynchronizationAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var last24Hours = now.AddHours(-24);

        var jobsRunning = await _dbContext.AzureDevOpsSyncJobs.CountAsync(item => item.Status == "Running", cancellationToken);
        var successfulRuns24h = await _dbContext.AzureDevOpsSyncJobs.CountAsync(item => item.Status == "Completed" && item.StartedAtUtc >= last24Hours, cancellationToken);
        var failedRuns24h = await _dbContext.AzureDevOpsSyncJobs.CountAsync(item => item.Status == "Failed" && item.StartedAtUtc >= last24Hours, cancellationToken);
        var queuedSchedules = await _dbContext.AutomationSchedules.CountAsync(item => item.IsEnabled && item.NextRunUtc >= now, cancellationToken);
        var lastSuccessfulSyncUtc = await _dbContext.AzureDevOpsSyncJobs
            .Where(item => item.Status == "Completed")
            .OrderByDescending(item => item.CompletedAtUtc)
            .Select(item => item.CompletedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var averageDurationSeconds = await _dbContext.AzureDevOpsSyncJobs
            .Where(item => item.DurationSeconds.HasValue)
            .AverageAsync(item => (double?)item.DurationSeconds, cancellationToken) ?? 0;

        var syncTypeBreakdown = await _dbContext.AzureDevOpsSyncJobs
            .AsNoTracking()
            .GroupBy(item => item.SyncType)
            .OrderByDescending(group => group.Count())
            .Select(group => new ItAdminChartPointDto
            {
                Label = group.Key,
                Value = group.Count(),
            })
            .ToListAsync(cancellationToken);

        var recentJobs = await GetRecentSyncJobsAsync(10, cancellationToken);

        return new ItAdminSynchronizationDto
        {
            JobsRunning = jobsRunning,
            SuccessfulRuns24h = successfulRuns24h,
            FailedRuns24h = failedRuns24h,
            QueuedSchedules = queuedSchedules,
            AverageDurationSeconds = decimal.Round((decimal)averageDurationSeconds, 2),
            LastSuccessfulSyncUtc = lastSuccessfulSyncUtc,
            GeneratedAtUtc = now,
            SyncTypeBreakdown = syncTypeBreakdown,
            RecentJobs = recentJobs,
        };
    }

    public async Task<ItAdminSyncMonitoringDto> GetSyncMonitoringAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var dayStart = now.Date;

        var totalJobsToday = await _dbContext.AzureDevOpsSyncJobs.CountAsync(item => item.StartedAtUtc >= dayStart, cancellationToken);
        var runningJobs = await _dbContext.AzureDevOpsSyncJobs.CountAsync(item => item.Status == "Running", cancellationToken);
        var failedJobs = await _dbContext.AzureDevOpsSyncJobs.CountAsync(item => item.Status == "Failed", cancellationToken);
        var warningLogs = await _dbContext.AdminLogEntries.CountAsync(item => item.Severity == "Warning", cancellationToken);
        var overdueSchedules = await _dbContext.AutomationSchedules.CountAsync(item => item.IsEnabled && item.NextRunUtc < now, cancellationToken);
        var activeMaintenanceWindows = await _dbContext.MaintenanceTasks.CountAsync(item => item.Status == "In Progress" || item.Status == "Scheduled", cancellationToken);

        var durationTrend = await _dbContext.AzureDevOpsSyncJobs
            .AsNoTracking()
            .Where(item => item.DurationSeconds.HasValue)
            .GroupBy(item => item.StartedAtUtc.Date)
            .OrderByDescending(group => group.Key)
            .Take(7)
            .Select(group => new ItAdminChartPointDto
            {
                Label = group.Key.ToString("dd MMM"),
                Value = decimal.Round((decimal)(group.Average(item => item.DurationSeconds) ?? 0), 2),
            })
            .ToListAsync(cancellationToken);
        durationTrend.Reverse();

        var statusBreakdown = await _dbContext.AzureDevOpsSyncJobs
            .AsNoTracking()
            .GroupBy(item => item.Status)
            .OrderByDescending(group => group.Count())
            .Select(group => new ItAdminChartPointDto
            {
                Label = group.Key,
                Value = group.Count(),
            })
            .ToListAsync(cancellationToken);

        var recentJobs = await GetRecentSyncJobsAsync(12, cancellationToken);

        return new ItAdminSyncMonitoringDto
        {
            TotalJobsToday = totalJobsToday,
            RunningJobs = runningJobs,
            FailedJobs = failedJobs,
            WarningLogs = warningLogs,
            OverdueSchedules = overdueSchedules,
            ActiveMaintenanceWindows = activeMaintenanceWindows,
            GeneratedAtUtc = now,
            DurationTrend = durationTrend,
            StatusBreakdown = statusBreakdown,
            RecentJobs = recentJobs,
        };
    }

    public async Task<ItAdminLogsDto> GetLogsAsync(string? severity, int take, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AdminLogEntries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(item => item.Severity == severity);
        }

        var items = await query
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(take)
            .Select(item => new ItAdminLogEntryDto
            {
                Id = item.Id,
                Severity = item.Severity,
                Category = item.Category,
                Message = item.Message,
                SourceSystem = item.SourceSystem,
                CorrelationId = item.CorrelationId,
                CreatedAtUtc = item.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var totalLogs = await _dbContext.AdminLogEntries.CountAsync(cancellationToken);
        var errorLogs = await _dbContext.AdminLogEntries.CountAsync(item => item.Severity == "Error" || item.Severity == "Critical", cancellationToken);
        var warningLogs = await _dbContext.AdminLogEntries.CountAsync(item => item.Severity == "Warning", cancellationToken);

        return new ItAdminLogsDto
        {
            TotalLogs = totalLogs,
            ErrorLogs = errorLogs,
            WarningLogs = warningLogs,
            GeneratedAtUtc = DateTime.UtcNow,
            Items = items,
        };
    }

    public async Task<ItAdminMaintenanceDto> GetMaintenanceAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var weekEnd = now.AddDays(7);

        var items = await _dbContext.MaintenanceTasks
            .AsNoTracking()
            .OrderBy(item => item.ScheduledStartUtc)
            .Select(item => new ItAdminMaintenanceTaskDto
            {
                Id = item.Id,
                Title = item.Title,
                EnvironmentName = item.EnvironmentName,
                Status = item.Status,
                OwnerName = item.OwnerName,
                Notes = item.Notes,
                IsRecurring = item.IsRecurring,
                ScheduledStartUtc = item.ScheduledStartUtc,
                ScheduledEndUtc = item.ScheduledEndUtc,
            })
            .ToListAsync(cancellationToken);

        return new ItAdminMaintenanceDto
        {
            ActiveTasks = items.Count(item => item.Status == "In Progress" || item.Status == "Scheduled"),
            ScheduledThisWeek = items.Count(item => item.ScheduledStartUtc >= now && item.ScheduledStartUtc <= weekEnd),
            OverdueTasks = items.Count(item => item.Status != "Completed" && item.ScheduledEndUtc.HasValue && item.ScheduledEndUtc.Value < now),
            GeneratedAtUtc = now,
            Items = items,
        };
    }

    public async Task<ItAdminSchedulingDto> GetSchedulingAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var items = await _dbContext.AutomationSchedules
            .AsNoTracking()
            .OrderBy(item => item.NextRunUtc)
            .Select(item => new ItAdminScheduleDto
            {
                Id = item.Id,
                Name = item.Name,
                JobType = item.JobType,
                ScopeName = item.ScopeName,
                CronExpression = item.CronExpression,
                TimeZoneId = item.TimeZoneId,
                RunWindow = item.RunWindow,
                IsEnabled = item.IsEnabled,
                FailureCount = item.FailureCount,
                LastRunUtc = item.LastRunUtc,
                NextRunUtc = item.NextRunUtc,
            })
            .ToListAsync(cancellationToken);

        return new ItAdminSchedulingDto
        {
            EnabledSchedules = items.Count(item => item.IsEnabled),
            DisabledSchedules = items.Count(item => !item.IsEnabled),
            OverdueSchedules = items.Count(item => item.IsEnabled && item.NextRunUtc.HasValue && item.NextRunUtc.Value < now),
            GeneratedAtUtc = now,
            Items = items,
        };
    }

    public async Task<TriggerItAdminSyncResponseDto> TriggerSynchronizationAsync(TriggerItAdminSyncRequestDto request, Guid? triggeredByUserId, string triggeredByDisplayName, CancellationToken cancellationToken = default)
    {
        var job = new AzureDevOpsSyncJob
        {
            SyncType = request.SyncType,
            ScopeName = request.ScopeName,
            Status = "Queued",
            Source = request.Source,
            TriggeredByDisplayName = string.IsNullOrWhiteSpace(triggeredByDisplayName) ? "IT Admin" : triggeredByDisplayName,
            TriggeredByUserId = triggeredByUserId,
            StartedAtUtc = DateTime.UtcNow,
        };

        _dbContext.AzureDevOpsSyncJobs.Add(job);
        _dbContext.AdminLogEntries.Add(new AdminLogEntry
        {
            Severity = "Information",
            Category = "Synchronization",
            Message = $"{request.SyncType} synchronization queued for {request.ScopeName}.",
            SourceSystem = "IT Admin",
            CreatedAtUtc = DateTime.UtcNow,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TriggerItAdminSyncResponseDto
        {
            JobId = job.Id,
            Status = job.Status,
            QueuedAtUtc = job.StartedAtUtc,
        };
    }

    private Task<List<ItAdminSyncJobDto>> GetRecentSyncJobsAsync(int take, CancellationToken cancellationToken)
    {
        return _dbContext.AzureDevOpsSyncJobs
            .AsNoTracking()
            .OrderByDescending(item => item.StartedAtUtc)
            .Take(take)
            .Select(item => new ItAdminSyncJobDto
            {
                JobId = item.Id,
                SyncType = item.SyncType,
                ScopeName = item.ScopeName,
                Status = item.Status,
                Source = item.Source,
                TriggeredByDisplayName = item.TriggeredByDisplayName,
                ItemsProcessed = item.ItemsProcessed,
                ItemsSucceeded = item.ItemsSucceeded,
                ItemsFailed = item.ItemsFailed,
                DurationSeconds = item.DurationSeconds,
                ErrorMessage = item.ErrorMessage,
                StartedAtUtc = item.StartedAtUtc,
                CompletedAtUtc = item.CompletedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ItAdminLoginStatisticsDto> GetLoginStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow.AddHours(-24);
        return new ItAdminLoginStatisticsDto
        {
            TotalLogins24Hours = await _dbContext.LoginAudits.CountAsync(item => item.LoginTime >= since, cancellationToken),
            ActiveSessions = await _dbContext.LoginAudits.CountAsync(item => item.LogoutTime == null && item.Status == "Success", cancellationToken),
            UniqueUsers24Hours = await _dbContext.LoginAudits.Where(item => item.LoginTime >= since).Select(item => item.UserId).Distinct().CountAsync(cancellationToken),
            LoggedOut24Hours = await _dbContext.LoginAudits.CountAsync(item => item.LogoutTime >= since, cancellationToken),
            GeneratedAtUtc = DateTime.UtcNow,
        };
    }

    public async Task<IReadOnlyList<ItAdminLoginAuditDto>> GetLoginAuditsAsync(int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoginAudits.AsNoTracking()
            .OrderByDescending(item => item.LoginTime)
            .Take(take)
            .Select(item => new ItAdminLoginAuditDto
            {
                AuditId = item.AuditId,
                UserId = item.UserId,
                UserName = item.User.Name,
                Email = item.User.Email,
                LoginTime = item.LoginTime,
                LogoutTime = item.LogoutTime,
                IpAddress = item.IPAddress,
                Status = item.Status,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ItAdminUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AsNoTracking().OrderBy(item => item.Name)
            .Select(item => new ItAdminUserDto
            {
                UserId = item.UserId,
                Name = item.Name,
                Email = item.Email,
                Role = item.Role.ToString(),
                LastLoginUtc = _dbContext.LoginAudits.Where(audit => audit.UserId == item.UserId).OrderByDescending(audit => audit.LoginTime).Select(audit => (DateTime?)audit.LoginTime).FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ItAdminUserDto?> UpdateUserRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<UserRole>(role, true, out var parsedRole)) return null;
        var user = await _dbContext.Users.SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (user is null) return null;
        user.Role = parsedRole;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ItAdminUserDto { UserId = user.UserId, Name = user.Name, Email = user.Email, Role = user.Role.ToString() };
    }

    public async Task<ItAdminSystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        var databaseReachable = await _dbContext.Database.CanConnectAsync(cancellationToken);
        var configuration = await _dbContext.AdminConfigurations.AsNoTracking().OrderByDescending(item => item.UpdatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        var runningJobs = await _dbContext.AzureDevOpsSyncJobs.CountAsync(item => item.Status == "Running", cancellationToken);
        return new ItAdminSystemHealthDto
        {
            Status = databaseReachable ? "Healthy" : "Degraded",
            DatabaseReachable = databaseReachable,
            AzureDevOpsConfigured = !string.IsNullOrWhiteSpace(configuration?.AzureDevOpsPatCipherText),
            SynchronizationEnabled = configuration?.AzureDevOpsSyncEnabled ?? false,
            RunningSynchronizationJobs = runningJobs,
            GeneratedAtUtc = DateTime.UtcNow,
        };
    }
}
