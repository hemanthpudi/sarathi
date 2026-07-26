using Sarathi.API.Application.DTOs.Dashboard;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Application.Services;

public class DashboardAnalyticsService : IDashboardAnalyticsService
{
    private readonly IDashboardAnalyticsRepository _dashboardAnalyticsRepository;
    private readonly IAzureDevOpsSynchronizationRuntimeService _azureDevOpsSyncRuntimeService;

    public DashboardAnalyticsService(
        IDashboardAnalyticsRepository dashboardAnalyticsRepository,
        IAzureDevOpsSynchronizationRuntimeService azureDevOpsSyncRuntimeService)
    {
        _dashboardAnalyticsRepository = dashboardAnalyticsRepository;
        _azureDevOpsSyncRuntimeService = azureDevOpsSyncRuntimeService;
    }

    public async Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        return await _dashboardAnalyticsRepository.GetMetricsAsync(cancellationToken);
    }

    public async Task<DashboardProjectStatisticsDto> GetProjectStatisticsAsync(int take, CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        var normalizedTake = take <= 0 ? 20 : Math.Min(take, 100);
        return await _dashboardAnalyticsRepository.GetProjectStatisticsAsync(normalizedTake, cancellationToken);
    }

    public async Task<DashboardSprintStatisticsDto> GetSprintStatisticsAsync(int take, CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        var normalizedTake = take <= 0 ? 20 : Math.Min(take, 100);
        return await _dashboardAnalyticsRepository.GetSprintStatisticsAsync(normalizedTake, cancellationToken);
    }

    public async Task<DashboardWorkItemSummariesDto> GetWorkItemSummariesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        return await _dashboardAnalyticsRepository.GetWorkItemSummariesAsync(cancellationToken);
    }

    public async Task<DashboardKpiCalculationsDto> GetKpiCalculationsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        return await _dashboardAnalyticsRepository.GetKpiCalculationsAsync(cancellationToken);
    }

    private async Task EnsureLiveDataAsync(CancellationToken cancellationToken)
    {
        if (!await _dashboardAnalyticsRepository.HasSynchronizedProjectDataAsync(cancellationToken))
        {
            await _azureDevOpsSyncRuntimeService.SynchronizeNowAsync(cancellationToken);
        }
    }
}