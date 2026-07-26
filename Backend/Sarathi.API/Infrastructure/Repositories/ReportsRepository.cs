using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.DTOs.Reports;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Persistence;

namespace Sarathi.API.Infrastructure.Repositories;

public class ReportsRepository : IReportsRepository
{
    private readonly AppDbContext _dbContext;

    public ReportsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReportsOverviewDto> GetOverviewAsync(Guid? userId, string? role, int? projectId = null, CancellationToken cancellationToken = default)
    {
        var accessibleProjectIds = await GetEffectiveProjectIdsAsync(userId, role, projectId, cancellationToken);
        var portfolioRows = await GetPortfolioRowsAsync(accessibleProjectIds, cancellationToken);
        var sprintRows = await GetSprintRowsAsync(accessibleProjectIds, cancellationToken);
        var workItemRows = await GetWorkItemRowsAsync(accessibleProjectIds, cancellationToken);
        var kpiRows = await GetKpiRowsAsync(accessibleProjectIds, cancellationToken);

        return new ReportsOverviewDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Sections = new List<ReportSectionDto>
            {
                BuildPortfolioSection(portfolioRows),
                BuildSprintSection(sprintRows),
                BuildWorkItemSection(workItemRows),
                BuildKpiSection(kpiRows),
            },
        };
    }

    public async Task<ExportReportResponseDto> ExportSectionAsync(string sectionKey, Guid? userId, string? role, int? projectId = null, CancellationToken cancellationToken = default)
    {
        var overview = await GetOverviewAsync(userId, role, projectId, cancellationToken);
        var section = overview.Sections.FirstOrDefault(item => item.Key == sectionKey)
            ?? overview.Sections.First(item => item.Key == "portfolio");

        var csv = new StringBuilder();
        csv.AppendLine(string.Join(',', section.Columns.Select(EscapeCsv)));
        foreach (var row in section.Rows)
        {
            csv.AppendLine(string.Join(',', section.Columns.Select(column => EscapeCsv(row.Cells.TryGetValue(column, out var value) ? value : string.Empty))));
        }

        return new ExportReportResponseDto
        {
            FileName = $"sarathi-{section.Key}-report-{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
            ContentType = "text/csv",
            ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(csv.ToString())),
        };
    }

    private async Task<List<PortfolioRow>> GetPortfolioRowsAsync(IReadOnlyCollection<int>? accessibleProjectIds, CancellationToken cancellationToken)
    {
        var query = _dbContext.Projects.AsNoTracking();
        if (accessibleProjectIds is { Count: > 0 })
        {
            query = query.Where(item => accessibleProjectIds.Contains(item.ProjectId));
        }

        return await query
            .OrderByDescending(item => item.LastUpdated)
            .Take(20)
            .Select(project => new PortfolioRow
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Visibility = project.Visibility,
                LastUpdated = project.LastUpdated,
                CompletionRate = _dbContext.KpiSnapshots
                    .Where(kpi => kpi.ProjectId == project.ProjectId)
                    .OrderByDescending(kpi => kpi.SnapshotDate)
                    .Select(kpi => kpi.CompletionRate)
                    .FirstOrDefault(),
                SprintVelocity = _dbContext.KpiSnapshots
                    .Where(kpi => kpi.ProjectId == project.ProjectId)
                    .OrderByDescending(kpi => kpi.SnapshotDate)
                    .Select(kpi => kpi.SprintVelocity)
                    .FirstOrDefault(),
                RiskScore = _dbContext.ProjectRiskAnalyses
                    .Where(risk => risk.ProjectId == project.ProjectId)
                    .OrderByDescending(risk => risk.GeneratedDate)
                    .Select(risk => (decimal?)risk.RiskScore)
                    .FirstOrDefault(),
                RiskLevel = _dbContext.ProjectRiskAnalyses
                    .Where(risk => risk.ProjectId == project.ProjectId)
                    .OrderByDescending(risk => risk.GeneratedDate)
                    .Select(risk => risk.RiskLevel)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<SprintRow>> GetSprintRowsAsync(IReadOnlyCollection<int>? accessibleProjectIds, CancellationToken cancellationToken)
    {
        var query = _dbContext.Sprints.AsNoTracking();
        if (accessibleProjectIds is { Count: > 0 })
        {
            query = query.Where(item => accessibleProjectIds.Contains(item.ProjectId));
        }

        return await query
            .OrderByDescending(item => item.EndDate)
            .Take(20)
            .Select(item => new SprintRow
            {
                SprintId = item.SprintId,
                ProjectName = item.Project != null ? item.Project.ProjectName : string.Empty,
                SprintName = item.SprintName,
                Status = item.Status,
                PlannedStoryPoints = item.PlannedStoryPoints,
                CompletedStoryPoints = item.CompletedStoryPoints,
                TotalWorkItems = item.TotalWorkItems,
                CompletedWorkItems = item.CompletedWorkItems,
                EndDate = item.EndDate,
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<WorkItemRow>> GetWorkItemRowsAsync(IReadOnlyCollection<int>? accessibleProjectIds, CancellationToken cancellationToken)
    {
        var query = _dbContext.WorkItems.AsNoTracking();
        if (accessibleProjectIds is { Count: > 0 })
        {
            query = query.Where(item => accessibleProjectIds.Contains(item.ProjectId));
        }

        return await query
            .OrderByDescending(item => item.LastUpdatedUtc)
            .Take(50)
            .Select(item => new WorkItemRow
            {
                ProjectName = item.Project != null ? item.Project.ProjectName : string.Empty,
                AzureWorkItemId = item.AzureWorkItemId,
                Title = item.Title,
                WorkItemType = item.WorkItemType,
                State = item.State,
                Priority = item.Priority,
                AssignedToName = item.AssignedToName,
                ProgressPercent = item.ProgressPercent,
                IsBlocked = item.IsBlocked,
                LastUpdatedUtc = item.LastUpdatedUtc,
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<KpiRow>> GetKpiRowsAsync(IReadOnlyCollection<int>? accessibleProjectIds, CancellationToken cancellationToken)
    {
        var query = _dbContext.KpiSnapshots.AsNoTracking();
        if (accessibleProjectIds is { Count: > 0 })
        {
            query = query.Where(item => accessibleProjectIds.Contains(item.ProjectId));
        }

        var rows = await query
            .GroupBy(item => item.SnapshotDate.Date)
            .OrderByDescending(group => group.Key)
            .Take(12)
            .Select(group => new KpiRow
            {
                SnapshotDate = group.Key,
                AverageCompletionRate = group.Average(item => item.CompletionRate) ?? 0,
                AverageSprintVelocity = group.Average(item => item.SprintVelocity) ?? 0,
                AverageDefectDensity = group.Average(item => item.DefectDensity) ?? 0,
                AverageBacklogHealth = group.Average(item => item.BacklogHealth) ?? 0,
                AverageReleaseSuccessRate = group.Average(item => item.ReleaseSuccessRate) ?? 0,
            })
            .ToListAsync(cancellationToken);

        rows.Reverse();
        return rows;
    }

    private async Task<IReadOnlyCollection<int>?> GetEffectiveProjectIdsAsync(Guid? userId, string? role, int? projectId, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<int>? accessibleProjectIds = null;

        if (string.Equals(role, "ProjectManager", StringComparison.OrdinalIgnoreCase) && userId is not null && userId != Guid.Empty)
        {
            accessibleProjectIds = await _dbContext.ProjectManagerAssignments
                .AsNoTracking()
                .Where(item => item.ProjectManagerUserId == userId.Value)
                .Select(item => item.ProjectId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        if (!projectId.HasValue)
        {
            return accessibleProjectIds;
        }

        if (accessibleProjectIds is null)
        {
            return new[] { projectId.Value };
        }

        return accessibleProjectIds.Contains(projectId.Value)
            ? new[] { projectId.Value }
            : Array.Empty<int>();
    }

    private static ReportSectionDto BuildPortfolioSection(List<PortfolioRow> rows)
    {
        return new ReportSectionDto
        {
            Key = "portfolio",
            Title = "Portfolio Overview",
            Description = "Cross-project visibility for leadership review and portfolio export.",
            Summaries = new List<ReportSectionSummaryDto>
            {
                new() { Label = "Projects", Value = rows.Count.ToString() },
                new() { Label = "High Risk", Value = rows.Count(item => string.Equals(item.RiskLevel, "High", StringComparison.OrdinalIgnoreCase) || string.Equals(item.RiskLevel, "Critical", StringComparison.OrdinalIgnoreCase)).ToString() },
                new() { Label = "Avg Completion", Value = rows.Count == 0 ? "0.0%" : $"{rows.Where(item => item.CompletionRate.HasValue).DefaultIfEmpty().Average(item => item?.CompletionRate ?? 0):0.0}%" },
            },
            ChartPoints = rows.Take(8).Select(item => new ReportSectionChartPointDto
            {
                Label = item.ProjectName,
                Value = item.CompletionRate ?? 0,
            }).ToList(),
            Columns = new List<string> { "Project", "Visibility", "Completion", "Velocity", "Risk Score", "Risk Level", "Last Updated" },
            Rows = rows.Select(item => new ReportTableRowDto
            {
                Cells = new Dictionary<string, string>
                {
                    ["Project"] = item.ProjectName,
                    ["Visibility"] = item.Visibility,
                    ["Completion"] = item.CompletionRate.HasValue ? $"{item.CompletionRate:0.0}%" : "-",
                    ["Velocity"] = item.SprintVelocity?.ToString("0.00") ?? "-",
                    ["Risk Score"] = item.RiskScore?.ToString("0.00") ?? "-",
                    ["Risk Level"] = item.RiskLevel ?? "-",
                    ["Last Updated"] = item.LastUpdated.ToString("g"),
                },
            }).ToList(),
        };
    }

    private static ReportSectionDto BuildSprintSection(List<SprintRow> rows)
    {
        return new ReportSectionDto
        {
            Key = "sprints",
            Title = "Sprint Performance",
            Description = "Recent sprint execution, delivery status, and completion history.",
            Summaries = new List<ReportSectionSummaryDto>
            {
                new() { Label = "Recent Sprints", Value = rows.Count.ToString() },
                new() { Label = "Active", Value = rows.Count(item => !string.Equals(item.Status, "Completed", StringComparison.OrdinalIgnoreCase)).ToString() },
                new() { Label = "Avg Completion", Value = rows.Count == 0 ? "0.0%" : $"{rows.Average(item => item.CompletionRate):0.0}%" },
            },
            ChartPoints = rows.Take(8).Select(item => new ReportSectionChartPointDto
            {
                Label = item.SprintName,
                Value = item.CompletionRate,
            }).ToList(),
            Columns = new List<string> { "Project", "Sprint", "Status", "Completion", "Story Points", "Work Items", "End Date" },
            Rows = rows.Select(item => new ReportTableRowDto
            {
                Cells = new Dictionary<string, string>
                {
                    ["Project"] = item.ProjectName,
                    ["Sprint"] = item.SprintName,
                    ["Status"] = item.Status,
                    ["Completion"] = $"{item.CompletionRate:0.0}%",
                    ["Story Points"] = $"{item.CompletedStoryPoints}/{item.PlannedStoryPoints}",
                    ["Work Items"] = $"{item.CompletedWorkItems}/{item.TotalWorkItems}",
                    ["End Date"] = item.EndDate.ToString("d"),
                },
            }).ToList(),
        };
    }

    private static ReportSectionDto BuildWorkItemSection(List<WorkItemRow> rows)
    {
        return new ReportSectionDto
        {
            Key = "workitems",
            Title = "Work Item Flow",
            Description = "Latest delivery throughput, blockers, and ownership signals from work items.",
            Summaries = new List<ReportSectionSummaryDto>
            {
                new() { Label = "Items", Value = rows.Count.ToString() },
                new() { Label = "Blocked", Value = rows.Count(item => item.IsBlocked).ToString() },
                new() { Label = "In Progress", Value = rows.Count(item => string.Equals(item.State, "In Progress", StringComparison.OrdinalIgnoreCase)).ToString() },
            },
            ChartPoints = rows.GroupBy(item => item.State)
                .OrderByDescending(group => group.Count())
                .Select(group => new ReportSectionChartPointDto { Label = group.Key, Value = group.Count() })
                .ToList(),
            Columns = new List<string> { "Project", "Work Item", "Type", "State", "Priority", "Assigned To", "Progress", "Updated" },
            Rows = rows.Select(item => new ReportTableRowDto
            {
                Cells = new Dictionary<string, string>
                {
                    ["Project"] = item.ProjectName,
                    ["Work Item"] = $"{item.AzureWorkItemId} • {item.Title}",
                    ["Type"] = item.WorkItemType,
                    ["State"] = item.IsBlocked ? "Blocked" : item.State,
                    ["Priority"] = item.Priority,
                    ["Assigned To"] = item.AssignedToName,
                    ["Progress"] = $"{item.ProgressPercent}%",
                    ["Updated"] = item.LastUpdatedUtc.ToString("g"),
                },
            }).ToList(),
        };
    }

    private static ReportSectionDto BuildKpiSection(List<KpiRow> rows)
    {
        return new ReportSectionDto
        {
            Key = "kpis",
            Title = "KPI Trends",
            Description = "Trend view for completion, velocity, defect density, backlog health, and release success.",
            Summaries = new List<ReportSectionSummaryDto>
            {
                new() { Label = "Avg Completion", Value = rows.Count == 0 ? "0.0%" : $"{rows.Average(item => item.AverageCompletionRate):0.0}%" },
                new() { Label = "Avg Velocity", Value = rows.Count == 0 ? "0.00" : $"{rows.Average(item => item.AverageSprintVelocity):0.00}" },
                new() { Label = "Avg Defect Density", Value = rows.Count == 0 ? "0.00" : $"{rows.Average(item => item.AverageDefectDensity):0.00}" },
                new() { Label = "Avg Backlog Health", Value = rows.Count == 0 ? "0.0%" : $"{rows.Average(item => item.AverageBacklogHealth):0.0}%" },
                new() { Label = "Avg Release Success", Value = rows.Count == 0 ? "0.0%" : $"{rows.Average(item => item.AverageReleaseSuccessRate):0.0}%" },
            },
            ChartPoints = rows.Select(item => new ReportSectionChartPointDto
            {
                Label = item.SnapshotDate.ToString("dd MMM"),
                Value = item.AverageCompletionRate,
            }).ToList(),
            Columns = new List<string> { "Snapshot", "Completion", "Velocity", "Defect Density", "Backlog Health", "Release Success" },
            Rows = rows.Select(item => new ReportTableRowDto
            {
                Cells = new Dictionary<string, string>
                {
                    ["Snapshot"] = item.SnapshotDate.ToString("d"),
                    ["Completion"] = $"{item.AverageCompletionRate:0.0}%",
                    ["Velocity"] = item.AverageSprintVelocity.ToString("0.00"),
                    ["Defect Density"] = item.AverageDefectDensity.ToString("0.00"),
                    ["Backlog Health"] = $"{item.AverageBacklogHealth:0.0}%",
                    ["Release Success"] = $"{item.AverageReleaseSuccessRate:0.0}%",
                },
            }).ToList(),
        };
    }

    private static string EscapeCsv(string value)
    {
        var normalized = value.Replace("\r", " ").Replace("\n", " ");
        return $"\"{normalized.Replace("\"", "\"\"")}\"";
    }

    private sealed class PortfolioRow
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public decimal? CompletionRate { get; set; }
        public decimal? SprintVelocity { get; set; }
        public decimal? RiskScore { get; set; }
        public string? RiskLevel { get; set; }
    }

    private sealed class SprintRow
    {
        public int SprintId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string SprintName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int PlannedStoryPoints { get; set; }
        public int CompletedStoryPoints { get; set; }
        public int TotalWorkItems { get; set; }
        public int CompletedWorkItems { get; set; }
        public DateTime EndDate { get; set; }
        public decimal CompletionRate => PlannedStoryPoints > 0 ? decimal.Round((decimal)CompletedStoryPoints / PlannedStoryPoints * 100, 2) : (TotalWorkItems > 0 ? decimal.Round((decimal)CompletedWorkItems / TotalWorkItems * 100, 2) : 0);
    }

    private sealed class WorkItemRow
    {
        public string ProjectName { get; set; } = string.Empty;
        public string AzureWorkItemId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string WorkItemType { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public int ProgressPercent { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
    }

    private sealed class KpiRow
    {
        public DateTime SnapshotDate { get; set; }
        public decimal AverageCompletionRate { get; set; }
        public decimal AverageSprintVelocity { get; set; }
        public decimal AverageDefectDensity { get; set; }
        public decimal AverageBacklogHealth { get; set; }
        public decimal AverageReleaseSuccessRate { get; set; }
    }
}