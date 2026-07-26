using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.DTOs.Dashboard;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Persistence;

namespace Sarathi.API.Infrastructure.Repositories;

public class DashboardAnalyticsRepository : IDashboardAnalyticsRepository
{
    private readonly AppDbContext _dbContext;

    public DashboardAnalyticsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var totalProjects = await _dbContext.Projects.CountAsync(cancellationToken);
        var activeSprints = await _dbContext.Sprints.CountAsync(item => item.Status != "Completed", cancellationToken);

        var workItemSummary = await _dbContext.WorkItems
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                OpenWorkItems = group.Count(item => item.State != "Done" && item.State != "Closed"),
                BlockedWorkItems = group.Count(item => item.IsBlocked || item.State == "Blocked"),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var runningSyncJobs = await _dbContext.AzureDevOpsSyncJobs.CountAsync(item => item.Status == "Running", cancellationToken);

        return new DashboardMetricsDto
        {
            TotalProjects = totalProjects,
            ActiveSprints = activeSprints,
            OpenWorkItems = workItemSummary?.OpenWorkItems ?? 0,
            BlockedWorkItems = workItemSummary?.BlockedWorkItems ?? 0,
            RunningSyncJobs = runningSyncJobs,
            GeneratedAtUtc = DateTime.UtcNow,
        };
    }

    public async Task<DashboardProjectStatisticsDto> GetProjectStatisticsAsync(int take, CancellationToken cancellationToken = default)
    {
        var projects = await _dbContext.Projects
            .AsNoTracking()
            .OrderByDescending(item => item.LastUpdated)
            .Take(take)
            .Select(project => new
            {
                project.ProjectId,
                project.ProjectName,
                project.Visibility,
                project.LastUpdated,
                LatestKpi = _dbContext.KpiSnapshots
                    .Where(kpi => kpi.ProjectId == project.ProjectId)
                    .OrderByDescending(kpi => kpi.SnapshotDate)
                    .Select(kpi => new
                    {
                        kpi.CompletionRate,
                        kpi.SprintVelocity,
                        kpi.DefectDensity,
                    })
                    .FirstOrDefault(),
                LatestRisk = _dbContext.ProjectRiskAnalyses
                    .Where(risk => risk.ProjectId == project.ProjectId)
                    .OrderByDescending(risk => risk.GeneratedDate)
                    .Select(risk => new
                    {
                        risk.RiskScore,
                        risk.RiskLevel,
                    })
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var items = projects.Select(project => new DashboardProjectStatisticItemDto
        {
            ProjectId = project.ProjectId,
            ProjectName = project.ProjectName,
            Visibility = project.Visibility,
            LastUpdated = project.LastUpdated,
            CompletionRate = project.LatestKpi?.CompletionRate,
            SprintVelocity = project.LatestKpi?.SprintVelocity,
            DefectDensity = project.LatestKpi?.DefectDensity,
            RiskScore = project.LatestRisk?.RiskScore,
            RiskLevel = project.LatestRisk?.RiskLevel,
        }).ToList();

        var totalProjects = await _dbContext.Projects.CountAsync(cancellationToken);
        var publicProjects = await _dbContext.Projects.CountAsync(item => item.Visibility == "Public", cancellationToken);
        var highRiskProjects = await _dbContext.ProjectRiskAnalyses
            .AsNoTracking()
            .Where(item => item.RiskLevel == "High" || item.RiskLevel == "Critical")
            .Select(item => item.ProjectId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new DashboardProjectStatisticsDto
        {
            TotalProjects = totalProjects,
            PublicProjects = publicProjects,
            PrivateProjects = Math.Max(totalProjects - publicProjects, 0),
            HighRiskProjects = highRiskProjects,
            GeneratedAtUtc = DateTime.UtcNow,
            Projects = items,
        };
    }

    public async Task<DashboardSprintStatisticsDto> GetSprintStatisticsAsync(int take, CancellationToken cancellationToken = default)
    {
        var sprintItems = await _dbContext.Sprints
            .AsNoTracking()
            .OrderByDescending(item => item.EndDate)
            .Take(take)
            .Select(item => new DashboardSprintStatisticItemDto
            {
                SprintId = item.SprintId,
                ProjectId = item.ProjectId,
                ProjectName = item.Project != null ? item.Project.ProjectName : string.Empty,
                SprintName = item.SprintName,
                Status = item.Status,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                PlannedStoryPoints = item.PlannedStoryPoints,
                CompletedStoryPoints = item.CompletedStoryPoints,
                TotalWorkItems = item.TotalWorkItems,
                CompletedWorkItems = item.CompletedWorkItems,
                CompletionRate = GetSprintCompletionRate(item.PlannedStoryPoints, item.CompletedStoryPoints, item.TotalWorkItems, item.CompletedWorkItems),
            })
            .ToListAsync(cancellationToken);

        var totalSprints = await _dbContext.Sprints.CountAsync(cancellationToken);
        var activeSprints = await _dbContext.Sprints.CountAsync(item => item.Status != "Completed", cancellationToken);
        var completedSprints = await _dbContext.Sprints.CountAsync(item => item.Status == "Completed", cancellationToken);
        var averageCompletionRate = sprintItems.Count == 0 ? 0 : decimal.Round(sprintItems.Average(item => item.CompletionRate), 2);

        return new DashboardSprintStatisticsDto
        {
            TotalSprints = totalSprints,
            ActiveSprints = activeSprints,
            CompletedSprints = completedSprints,
            AverageCompletionRate = averageCompletionRate,
            GeneratedAtUtc = DateTime.UtcNow,
            Sprints = sprintItems,
        };
    }

    public async Task<DashboardWorkItemSummariesDto> GetWorkItemSummariesAsync(CancellationToken cancellationToken = default)
    {
        var totalWorkItems = await _dbContext.WorkItems.CountAsync(cancellationToken);

        var summary = await _dbContext.WorkItems
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                OpenWorkItems = group.Count(item => item.State != "Done" && item.State != "Closed"),
                InProgressWorkItems = group.Count(item => item.State == "In Progress"),
                BlockedWorkItems = group.Count(item => item.IsBlocked || item.State == "Blocked"),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var byState = await _dbContext.WorkItems
            .AsNoTracking()
            .GroupBy(item => item.State)
            .OrderByDescending(group => group.Count())
            .Select(group => new DashboardWorkItemSummaryItemDto
            {
                Label = group.Key,
                Count = group.Count(),
            })
            .ToListAsync(cancellationToken);

        var byType = await _dbContext.WorkItems
            .AsNoTracking()
            .GroupBy(item => item.WorkItemType)
            .OrderByDescending(group => group.Count())
            .Select(group => new DashboardWorkItemSummaryItemDto
            {
                Label = group.Key,
                Count = group.Count(),
            })
            .ToListAsync(cancellationToken);

        return new DashboardWorkItemSummariesDto
        {
            TotalWorkItems = totalWorkItems,
            OpenWorkItems = summary?.OpenWorkItems ?? 0,
            InProgressWorkItems = summary?.InProgressWorkItems ?? 0,
            BlockedWorkItems = summary?.BlockedWorkItems ?? 0,
            GeneratedAtUtc = DateTime.UtcNow,
            ByState = byState,
            ByType = byType,
        };
    }

    public async Task<DashboardKpiCalculationsDto> GetKpiCalculationsAsync(CancellationToken cancellationToken = default)
    {
        var aggregate = await _dbContext.KpiSnapshots
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                AverageCompletionRate = group.Average(item => item.CompletionRate) ?? 0,
                AverageSprintVelocity = group.Average(item => item.SprintVelocity) ?? 0,
                AverageDefectDensity = group.Average(item => item.DefectDensity) ?? 0,
                AverageBacklogHealth = group.Average(item => item.BacklogHealth) ?? 0,
                AverageReleaseSuccessRate = group.Average(item => item.ReleaseSuccessRate) ?? 0,
            })
            .FirstOrDefaultAsync(cancellationToken);

        var completionTrend = await _dbContext.KpiSnapshots
            .AsNoTracking()
            .Where(item => item.CompletionRate.HasValue)
            .GroupBy(item => item.SnapshotDate.Date)
            .OrderByDescending(group => group.Key)
            .Take(8)
            .Select(group => new DashboardChartPointDto
            {
                Label = group.Key.ToString("dd MMM"),
                Value = decimal.Round(group.Average(item => item.CompletionRate) ?? 0, 2),
            })
            .ToListAsync(cancellationToken);
        completionTrend.Reverse();

        var velocityTrend = await _dbContext.KpiSnapshots
            .AsNoTracking()
            .Where(item => item.SprintVelocity.HasValue)
            .GroupBy(item => item.SnapshotDate.Date)
            .OrderByDescending(group => group.Key)
            .Take(8)
            .Select(group => new DashboardChartPointDto
            {
                Label = group.Key.ToString("dd MMM"),
                Value = decimal.Round(group.Average(item => item.SprintVelocity) ?? 0, 2),
            })
            .ToListAsync(cancellationToken);
        velocityTrend.Reverse();

        return new DashboardKpiCalculationsDto
        {
            AverageCompletionRate = decimal.Round(aggregate?.AverageCompletionRate ?? 0, 2),
            AverageSprintVelocity = decimal.Round(aggregate?.AverageSprintVelocity ?? 0, 2),
            AverageDefectDensity = decimal.Round(aggregate?.AverageDefectDensity ?? 0, 2),
            AverageBacklogHealth = decimal.Round(aggregate?.AverageBacklogHealth ?? 0, 2),
            AverageReleaseSuccessRate = decimal.Round(aggregate?.AverageReleaseSuccessRate ?? 0, 2),
            GeneratedAtUtc = DateTime.UtcNow,
            CompletionTrend = completionTrend,
            VelocityTrend = velocityTrend,
        };
    }

    private static decimal GetSprintCompletionRate(int plannedStoryPoints, int completedStoryPoints, int totalWorkItems, int completedWorkItems)
    {
        if (plannedStoryPoints > 0)
        {
            return decimal.Round((decimal)completedStoryPoints / plannedStoryPoints * 100, 2);
        }

        if (totalWorkItems > 0)
        {
            return decimal.Round((decimal)completedWorkItems / totalWorkItems * 100, 2);
        }

        return 0;
    }
}