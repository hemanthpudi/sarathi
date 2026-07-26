using Sarathi.API.Application.DTOs.AzureDevOps;

namespace Sarathi.API.Application.Interfaces;

public interface IAzureDevOpsIntegrationService
{
    Task<AzureDevOpsIntegrationConfigurationDto> GetConfigurationAsync(CancellationToken cancellationToken = default);

    Task<AzureDevOpsIntegrationConfigurationDto> UpdateConfigurationAsync(UpdateAzureDevOpsIntegrationConfigurationRequestDto request, Guid? updatedByUserId, CancellationToken cancellationToken = default);

    Task<AzureDevOpsLiveSummaryDto> GetLiveSummaryAsync(CancellationToken cancellationToken = default);

    Task<AzureDevOpsConnectionTestResultDto> TestConnectionAsync(TestAzureDevOpsConnectionRequestDto request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AzureDevOpsScheduleDto>> GetSchedulesAsync(CancellationToken cancellationToken = default);

    Task<AzureDevOpsScheduleDto> UpsertScheduleAsync(UpsertAzureDevOpsScheduleRequestDto request, CancellationToken cancellationToken = default);

    Task<AzureDevOpsSyncQueueResponseDto> QueueSynchronizationAsync(QueueAzureDevOpsSyncRequestDto request, Guid? triggeredByUserId, string triggeredByDisplayName, CancellationToken cancellationToken = default);

    Task<AzureDevOpsSyncJobsDto> GetSyncJobsAsync(int take, CancellationToken cancellationToken = default);

    Task<AzureDevOpsLogsDto> GetLogsAsync(int take, CancellationToken cancellationToken = default);
}