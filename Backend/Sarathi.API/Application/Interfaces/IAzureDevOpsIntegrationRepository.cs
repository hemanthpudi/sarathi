using Sarathi.API.Application.DTOs.AzureDevOps;
using Sarathi.API.Domain.Entities;

namespace Sarathi.API.Application.Interfaces;

public interface IAzureDevOpsIntegrationRepository
{
    Task<IReadOnlyList<AzureDevOpsScheduleDto>> GetSchedulesAsync(CancellationToken cancellationToken = default);

    Task<AzureDevOpsScheduleDto> UpsertScheduleAsync(UpsertAzureDevOpsScheduleRequestDto request, CancellationToken cancellationToken = default);

    Task<AzureDevOpsSyncQueueResponseDto> QueueSynchronizationAsync(QueueAzureDevOpsSyncRequestDto request, Guid? triggeredByUserId, string triggeredByDisplayName, CancellationToken cancellationToken = default);

    Task<AzureDevOpsSyncJobsDto> GetSyncJobsAsync(int take, CancellationToken cancellationToken = default);

    Task<AzureDevOpsLogsDto> GetLogsAsync(int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutomationSchedule>> GetDueSchedulesAsync(DateTime utcNow, CancellationToken cancellationToken = default);

    Task QueueScheduleJobAsync(AutomationSchedule schedule, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AzureDevOpsSyncJob>> GetQueuedJobsAsync(int take, CancellationToken cancellationToken = default);

    Task<AzureDevOpsSyncJob?> GetJobAsync(int jobId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task LogAsync(string severity, string category, string message, string sourceSystem, string? correlationId, CancellationToken cancellationToken = default);
}