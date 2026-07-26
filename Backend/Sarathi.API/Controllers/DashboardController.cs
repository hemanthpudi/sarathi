using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sarathi.API.Application.DTOs.Dashboard;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardAnalyticsService _dashboardAnalyticsService;

    public DashboardController(IDashboardAnalyticsService dashboardAnalyticsService)
    {
        _dashboardAnalyticsService = dashboardAnalyticsService;
    }

    [HttpGet("metrics")]
    [ProducesResponseType(typeof(DashboardMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var response = await _dashboardAnalyticsService.GetMetricsAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("project-statistics")]
    [ProducesResponseType(typeof(DashboardProjectStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProjectStatistics([FromQuery] int take = 20, CancellationToken cancellationToken = default)
    {
        var response = await _dashboardAnalyticsService.GetProjectStatisticsAsync(take, cancellationToken);
        return Ok(response);
    }

    [HttpGet("sprint-statistics")]
    [ProducesResponseType(typeof(DashboardSprintStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSprintStatistics([FromQuery] int take = 20, CancellationToken cancellationToken = default)
    {
        var response = await _dashboardAnalyticsService.GetSprintStatisticsAsync(take, cancellationToken);
        return Ok(response);
    }

    [HttpGet("work-item-summaries")]
    [ProducesResponseType(typeof(DashboardWorkItemSummariesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWorkItemSummaries(CancellationToken cancellationToken)
    {
        var response = await _dashboardAnalyticsService.GetWorkItemSummariesAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("kpi-calculations")]
    [ProducesResponseType(typeof(DashboardKpiCalculationsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetKpiCalculations(CancellationToken cancellationToken)
    {
        var response = await _dashboardAnalyticsService.GetKpiCalculationsAsync(cancellationToken);
        return Ok(response);
    }
}
