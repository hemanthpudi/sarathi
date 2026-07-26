using Sarathi.API.Application.DTOs.Dashboard;

namespace Sarathi.API.Application.Interfaces;

public interface IDashboardAnalyticsService
{
    Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task<DashboardProjectStatisticsDto> GetProjectStatisticsAsync(int take, CancellationToken cancellationToken = default);

    Task<DashboardSprintStatisticsDto> GetSprintStatisticsAsync(int take, CancellationToken cancellationToken = default);

    Task<DashboardWorkItemSummariesDto> GetWorkItemSummariesAsync(CancellationToken cancellationToken = default);

    Task<DashboardKpiCalculationsDto> GetKpiCalculationsAsync(CancellationToken cancellationToken = default);
}