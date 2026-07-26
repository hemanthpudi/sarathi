using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.DTOs.Admin;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Persistence;

namespace Sarathi.API.Infrastructure.Repositories;

public class AdminProjectStatisticsRepository : IAdminProjectStatisticsRepository
{
    private readonly AppDbContext _dbContext;

    public AdminProjectStatisticsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminProjectStatisticsDto> GetProjectStatisticsAsync(int take, CancellationToken cancellationToken = default)
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

        var items = projects.Select(project => new AdminProjectStatisticItemDto
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

        var kpiAggregate = await _dbContext.KpiSnapshots
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                AverageCompletionRate = group.Average(item => item.CompletionRate) ?? 0,
                AverageSprintVelocity = group.Average(item => item.SprintVelocity) ?? 0,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new AdminProjectStatisticsDto
        {
            TotalProjects = totalProjects,
            PublicProjects = publicProjects,
            PrivateProjects = Math.Max(totalProjects - publicProjects, 0),
            HighRiskProjects = highRiskProjects,
            AverageCompletionRate = decimal.Round(kpiAggregate?.AverageCompletionRate ?? 0, 2),
            AverageSprintVelocity = decimal.Round(kpiAggregate?.AverageSprintVelocity ?? 0, 2),
            GeneratedAtUtc = DateTime.UtcNow,
            Projects = items,
        };
    }
}
