using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.DTOs.AzureDevOps;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Domain.Entities;
using Sarathi.API.Persistence;

namespace Sarathi.API.Infrastructure.Repositories;

public class AzureDevOpsIntegrationRepository : IAzureDevOpsIntegrationRepository
{
    private readonly AppDbContext _dbContext;

    public AzureDevOpsIntegrationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AzureDevOpsScheduleDto>> GetSchedulesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.AutomationSchedules
            .AsNoTracking()
            .Where(item => item.Source == "Azure DevOps")
            .OrderBy(item => item.NextRunUtc)
            .Select(item => new AzureDevOpsScheduleDto
            {
                Id = item.Id,
                Name = item.Name,
                SyncType = item.JobType,
                ScopeName = item.ScopeName,
                Source = item.Source,
                IntervalMinutes = item.IntervalMinutes,
                IsEnabled = item.IsEnabled,
                LastRunUtc = item.LastRunUtc,
                NextRunUtc = item.NextRunUtc,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AzureDevOpsScheduleDto> UpsertScheduleAsync(UpsertAzureDevOpsScheduleRequestDto request, CancellationToken cancellationToken = default)
    {
        AutomationSchedule schedule;
        if (request.Id.HasValue)
        {
            schedule = await _dbContext.AutomationSchedules.FirstOrDefaultAsync(item => item.Id == request.Id.Value, cancellationToken)
                ?? new AutomationSchedule();
            if (schedule.Id == 0)
            {
                _dbContext.AutomationSchedules.Add(schedule);
            }
        }
        else
        {
            schedule = new AutomationSchedule();
            _dbContext.AutomationSchedules.Add(schedule);
        }

        schedule.Name = request.Name.Trim();
        schedule.JobType = request.SyncType.Trim();
        schedule.ScopeName = request.ScopeName.Trim();
        schedule.Source = "Azure DevOps";
        schedule.CronExpression = $"every-{request.IntervalMinutes}-minutes";
        schedule.IntervalMinutes = request.IntervalMinutes;
        schedule.TimeZoneId = "UTC";
        schedule.RunWindow = "Always";
        schedule.IsEnabled = request.IsEnabled;
        schedule.NextRunUtc ??= DateTime.UtcNow.AddMinutes(request.IntervalMinutes);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AzureDevOpsScheduleDto
        {
            Id = schedule.Id,
            Name = schedule.Name,
            SyncType = schedule.JobType,
            ScopeName = schedule.ScopeName,
            Source = schedule.Source,
            IntervalMinutes = schedule.IntervalMinutes,
            IsEnabled = schedule.IsEnabled,
            LastRunUtc = schedule.LastRunUtc,
            NextRunUtc = schedule.NextRunUtc,
        };
    }

    public async Task<AzureDevOpsSyncQueueResponseDto> QueueSynchronizationAsync(QueueAzureDevOpsSyncRequestDto request, Guid? triggeredByUserId, string triggeredByDisplayName, CancellationToken cancellationToken = default)
    {
        var job = new AzureDevOpsSyncJob
        {
            SyncType = request.SyncType,
            ScopeName = request.ScopeName,
            Status = "Queued",
            Source = request.Source,
            TriggeredByDisplayName = string.IsNullOrWhiteSpace(triggeredByDisplayName) ? "System" : triggeredByDisplayName,
            TriggeredByUserId = triggeredByUserId,
            StartedAtUtc = DateTime.UtcNow,
        };

        _dbContext.AzureDevOpsSyncJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await LogAsync(
            "Information",
            "AzureDevOpsSynchronization",
            $"Queued {request.SyncType} synchronization for {request.ScopeName}.",
            "Azure DevOps Integration",
            job.Id.ToString(),
            cancellationToken);

        return new AzureDevOpsSyncQueueResponseDto
        {
            JobId = job.Id,
            Status = job.Status,
            QueuedAtUtc = job.StartedAtUtc,
        };
    }

    public async Task<AzureDevOpsSyncJobsDto> GetSyncJobsAsync(int take, CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.AzureDevOpsSyncJobs
            .AsNoTracking()
            .OrderByDescending(item => item.StartedAtUtc)
            .Take(take)
            .Select(item => new AzureDevOpsSyncJobSummaryDto
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

        return new AzureDevOpsSyncJobsDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Items = items,
        };
    }

    public async Task<AzureDevOpsLogsDto> GetLogsAsync(int take, CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.AdminLogEntries
            .AsNoTracking()
            .Where(item => item.SourceSystem == "Azure DevOps Integration")
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(take)
            .Select(item => new AzureDevOpsLogEntryDto
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

        return new AzureDevOpsLogsDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Items = items,
        };
    }

    public async Task<IReadOnlyList<AutomationSchedule>> GetDueSchedulesAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AutomationSchedules
            .Where(item => item.Source == "Azure DevOps" && item.IsEnabled && item.NextRunUtc.HasValue && item.NextRunUtc <= utcNow)
            .OrderBy(item => item.NextRunUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task QueueScheduleJobAsync(AutomationSchedule schedule, CancellationToken cancellationToken = default)
    {
        _dbContext.AzureDevOpsSyncJobs.Add(new AzureDevOpsSyncJob
        {
            SyncType = schedule.JobType,
            ScopeName = schedule.ScopeName,
            Status = "Queued",
            Source = "Schedule",
            TriggeredByDisplayName = schedule.Name,
            StartedAtUtc = DateTime.UtcNow,
        });

        schedule.LastRunUtc = DateTime.UtcNow;
        schedule.NextRunUtc = DateTime.UtcNow.AddMinutes(Math.Max(schedule.IntervalMinutes, 5));
        await _dbContext.SaveChangesAsync(cancellationToken);

        await LogAsync(
            "Information",
            "AzureDevOpsScheduling",
            $"Schedule {schedule.Name} queued a {schedule.JobType} synchronization for {schedule.ScopeName}.",
            "Azure DevOps Integration",
            null,
            cancellationToken);
    }

    public async Task<IReadOnlyList<AzureDevOpsSyncJob>> GetQueuedJobsAsync(int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AzureDevOpsSyncJobs
            .Where(item => item.Status == "Queued")
            .OrderBy(item => item.StartedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<AzureDevOpsSyncJob?> GetJobAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AzureDevOpsSyncJobs.FirstOrDefaultAsync(item => item.Id == jobId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task LogAsync(string severity, string category, string message, string sourceSystem, string? correlationId, CancellationToken cancellationToken = default)
    {
        _dbContext.AdminLogEntries.Add(new AdminLogEntry
        {
            Severity = severity,
            Category = category,
            Message = message,
            SourceSystem = sourceSystem,
            CorrelationId = correlationId,
            CreatedAtUtc = DateTime.UtcNow,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}