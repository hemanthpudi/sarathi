using Sarathi.API.Application.DTOs.Dashboard;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Application.Services;

public class DashboardAnalyticsService : IDashboardAnalyticsService
{
    private readonly IDashboardAnalyticsRepository _dashboardAnalyticsRepository;

    public DashboardAnalyticsService(IDashboardAnalyticsRepository dashboardAnalyticsRepository)
    {
        _dashboardAnalyticsRepository = dashboardAnalyticsRepository;
    }

    public Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        return _dashboardAnalyticsRepository.GetMetricsAsync(cancellationToken);
    }

    public Task<DashboardProjectStatisticsDto> GetProjectStatisticsAsync(int take, CancellationToken cancellationToken = default)
    {
        var normalizedTake = take <= 0 ? 20 : Math.Min(take, 100);
        return _dashboardAnalyticsRepository.GetProjectStatisticsAsync(normalizedTake, cancellationToken);
    }

    public Task<DashboardSprintStatisticsDto> GetSprintStatisticsAsync(int take, CancellationToken cancellationToken = default)
    {
        var normalizedTake = take <= 0 ? 20 : Math.Min(take, 100);
        return _dashboardAnalyticsRepository.GetSprintStatisticsAsync(normalizedTake, cancellationToken);
    }

    public Task<DashboardWorkItemSummariesDto> GetWorkItemSummariesAsync(CancellationToken cancellationToken = default)
    {
        return _dashboardAnalyticsRepository.GetWorkItemSummariesAsync(cancellationToken);
    }

    public Task<DashboardKpiCalculationsDto> GetKpiCalculationsAsync(CancellationToken cancellationToken = default)
    {
        return _dashboardAnalyticsRepository.GetKpiCalculationsAsync(cancellationToken);
    }
}