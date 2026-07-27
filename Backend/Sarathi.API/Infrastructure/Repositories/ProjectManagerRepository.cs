using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.DTOs.ProjectManager;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Persistence;

namespace Sarathi.API.Infrastructure.Repositories;

public class ProjectManagerRepository : IProjectManagerRepository
{
    private readonly AppDbContext _dbContext;

    public ProjectManagerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IsProjectAssignedAsync(Guid userId, int projectId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ProjectManagerAssignments
            .AsNoTracking()
            .AnyAsync(item => item.ProjectManagerUserId == userId && item.ProjectId == projectId, cancellationToken);
    }

    public async Task<ProjectManagerDashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var projectIds = await GetAssignedProjectIdsAsync(userId, cancellationToken);
        if (projectIds.Count == 0)
        {
            return new ProjectManagerDashboardDto { GeneratedAtUtc = DateTime.UtcNow };
        }

        var assignedProjects = await GetAssignedProjectsAsync(userId, cancellationToken);

        var activeSprints = await _dbContext.Sprints
            .AsNoTracking()
            .CountAsync(item => projectIds.Contains(item.ProjectId) && item.Status != "Completed", cancellationToken);

        var workItemSummary = await _dbContext.WorkItems
            .AsNoTracking()
            .Where(item => projectIds.Contains(item.ProjectId))
            .GroupBy(_ => 1)
            .Select(group => new
            {
                OpenWorkItems = group.Count(item => item.State != "Done" && item.State != "Closed"),
                BlockedWorkItems = group.Count(item => item.IsBlocked || item.State == "Blocked"),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var kpiAggregate = await _dbContext.KpiSnapshots
            .AsNoTracking()
            .Where(item => projectIds.Contains(item.ProjectId))
            .GroupBy(_ => 1)
            .Select(group => new
            {
                AverageCompletionRate = group.Average(item => item.CompletionRate) ?? 0,
                AverageSprintVelocity = group.Average(item => item.SprintVelocity) ?? 0,
            })
            .FirstOrDefaultAsync(cancellationToken);

        var velocityTrend = await _dbContext.KpiSnapshots
            .AsNoTracking()
            .Where(item => projectIds.Contains(item.ProjectId) && item.SprintVelocity.HasValue)
            .GroupBy(item => item.SnapshotDate.Date)
            .OrderByDescending(group => group.Key)
            .Take(6)
            .Select(group => new ProjectManagerChartPointDto
            {
                Label = group.Key.ToString("dd MMM"),
                Value = decimal.Round(group.Average(item => item.SprintVelocity) ?? 0, 2),
            })
            .ToListAsync(cancellationToken);

        velocityTrend.Reverse();

        var workItemsByState = await _dbContext.WorkItems
            .AsNoTracking()
            .Where(item => projectIds.Contains(item.ProjectId))
            .GroupBy(item => item.State)
            .OrderByDescending(group => group.Count())
            .Select(group => new ProjectManagerChartPointDto
            {
                Label = group.Key,
                Value = group.Count(),
            })
            .ToListAsync(cancellationToken);

        return new ProjectManagerDashboardDto
        {
            AssignedProjects = assignedProjects.Count,
            ActiveSprints = activeSprints,
            OpenWorkItems = workItemSummary?.OpenWorkItems ?? 0,
            BlockedWorkItems = workItemSummary?.BlockedWorkItems ?? 0,
            AverageCompletionRate = decimal.Round(kpiAggregate?.AverageCompletionRate ?? 0, 2),
            AverageSprintVelocity = decimal.Round(kpiAggregate?.AverageSprintVelocity ?? 0, 2),
            GeneratedAtUtc = DateTime.UtcNow,
            SprintVelocityTrend = velocityTrend,
            WorkItemsByState = workItemsByState,
            SpotlightProjects = assignedProjects.Take(4).ToList(),
        };
    }

    public async Task<IReadOnlyList<ProjectManagerAssignedProjectDto>> GetAssignedProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectManagerAssignments
            .AsNoTracking()
            .Where(item => item.ProjectManagerUserId == userId)
            .OrderByDescending(item => item.AssignedAtUtc)
            .Select(assignment => new ProjectManagerAssignedProjectDto
            {
                ProjectId = assignment.ProjectId,
                ProjectName = assignment.Project != null ? assignment.Project.ProjectName : string.Empty,
                Visibility = assignment.Project != null ? assignment.Project.Visibility : string.Empty,
                ProjectRole = assignment.ProjectRole,
                AllocationPercent = assignment.AllocationPercent,
                DeliveryHealth = assignment.DeliveryHealth,
                CompletionRate = _dbContext.KpiSnapshots
                    .Where(kpi => kpi.ProjectId == assignment.ProjectId)
                    .OrderByDescending(kpi => kpi.SnapshotDate)
                    .Select(kpi => kpi.CompletionRate)
                    .FirstOrDefault(),
                SprintVelocity = _dbContext.KpiSnapshots
                    .Where(kpi => kpi.ProjectId == assignment.ProjectId)
                    .OrderByDescending(kpi => kpi.SnapshotDate)
                    .Select(kpi => kpi.SprintVelocity)
                    .FirstOrDefault(),
                RiskScore = _dbContext.ProjectRiskAnalyses
                    .Where(risk => risk.ProjectId == assignment.ProjectId)
                    .OrderByDescending(risk => risk.GeneratedDate)
                    .Select(risk => (decimal?)risk.RiskScore)
                    .FirstOrDefault(),
                RiskLevel = _dbContext.ProjectRiskAnalyses
                    .Where(risk => risk.ProjectId == assignment.ProjectId)
                    .OrderByDescending(risk => risk.GeneratedDate)
                    .Select(risk => risk.RiskLevel)
                    .FirstOrDefault(),
                BlockedItems = _dbContext.WorkItems.Count(workItem => workItem.ProjectId == assignment.ProjectId && (workItem.IsBlocked || workItem.State == "Blocked")),
                LastUpdated = assignment.Project != null ? assignment.Project.LastUpdated : assignment.AssignedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasSynchronizedProjectDataAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects.AsNoTracking().AnyAsync(cancellationToken);
    }

    public async Task<ProjectManagerSprintProgressDto> GetSprintProgressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var projectIds = await GetAssignedProjectIdsAsync(userId, cancellationToken);
        if (projectIds.Count == 0)
        {
            return new ProjectManagerSprintProgressDto { GeneratedAtUtc = DateTime.UtcNow };
        }

        var sprints = await _dbContext.Sprints
            .AsNoTracking()
            .Where(item => projectIds.Contains(item.ProjectId))
            .OrderBy(item => item.EndDate)
            .Select(item => new ProjectManagerSprintItemDto
            {
                SprintId = item.SprintId,
                ProjectId = item.ProjectId,
                ProjectName = item.Project != null ? item.Project.ProjectName : string.Empty,
                SprintName = item.SprintName,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                PlannedStoryPoints = item.PlannedStoryPoints,
                CompletedStoryPoints = item.CompletedStoryPoints,
                TotalWorkItems = item.TotalWorkItems,
                CompletedWorkItems = item.CompletedWorkItems,
                CompletionRate = GetSprintCompletionRate(item.PlannedStoryPoints, item.CompletedStoryPoints, item.TotalWorkItems, item.CompletedWorkItems),
                Status = item.Status,
            })
            .ToListAsync(cancellationToken);

        return new ProjectManagerSprintProgressDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Sprints = sprints,
        };
    }

    public async Task<ProjectManagerWorkItemsDto> GetWorkItemsAsync(Guid userId, int? projectId, string? state, int take, CancellationToken cancellationToken = default)
    {
        var projectIds = await GetAssignedProjectIdsAsync(userId, cancellationToken);
        if (projectIds.Count == 0)
        {
            return new ProjectManagerWorkItemsDto { GeneratedAtUtc = DateTime.UtcNow };
        }

        var query = _dbContext.WorkItems
            .AsNoTracking()
            .Where(item => projectIds.Contains(item.ProjectId));

        if (projectId.HasValue)
        {
            query = query.Where(item => item.ProjectId == projectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            query = query.Where(item => item.State == state);
        }

        var summary = await query
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalWorkItems = group.Count(),
                OpenWorkItems = group.Count(item => item.State != "Done" && item.State != "Closed"),
                InProgressWorkItems = group.Count(item => item.State == "In Progress"),
                BlockedWorkItems = group.Count(item => item.IsBlocked || item.State == "Blocked"),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var items = await query
            .OrderByDescending(item => item.LastUpdatedUtc)
            .Take(take)
            .Select(item => new ProjectManagerWorkItemDto
            {
                WorkItemId = item.WorkItemId,
                ProjectId = item.ProjectId,
                ProjectName = item.Project != null ? item.Project.ProjectName : string.Empty,
                AzureWorkItemId = item.AzureWorkItemId,
                Title = item.Title,
                WorkItemType = item.WorkItemType,
                State = item.State,
                Priority = item.Priority,
                AssignedToName = item.AssignedToName,
                StoryPoints = item.StoryPoints,
                ProgressPercent = item.ProgressPercent,
                IsBlocked = item.IsBlocked,
                SprintName = item.Sprint != null ? item.Sprint.SprintName : string.Empty,
                LastUpdatedUtc = item.LastUpdatedUtc,
            })
            .ToListAsync(cancellationToken);

        return new ProjectManagerWorkItemsDto
        {
            TotalWorkItems = summary?.TotalWorkItems ?? 0,
            OpenWorkItems = summary?.OpenWorkItems ?? 0,
            InProgressWorkItems = summary?.InProgressWorkItems ?? 0,
            BlockedWorkItems = summary?.BlockedWorkItems ?? 0,
            GeneratedAtUtc = DateTime.UtcNow,
            Items = items,
        };
    }

    public async Task<ProjectManagerKpisDto> GetKpisAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var projectIds = await GetAssignedProjectIdsAsync(userId, cancellationToken);
        if (projectIds.Count == 0)
        {
            return new ProjectManagerKpisDto { GeneratedAtUtc = DateTime.UtcNow };
        }

        var aggregate = await _dbContext.KpiSnapshots
            .AsNoTracking()
            .Where(item => projectIds.Contains(item.ProjectId))
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
            .Where(item => projectIds.Contains(item.ProjectId) && item.CompletionRate.HasValue)
            .GroupBy(item => item.SnapshotDate.Date)
            .OrderByDescending(group => group.Key)
            .Take(8)
            .Select(group => new ProjectManagerChartPointDto
            {
                Label = group.Key.ToString("dd MMM"),
                Value = decimal.Round(group.Average(item => item.CompletionRate) ?? 0, 2),
            })
            .ToListAsync(cancellationToken);

        completionTrend.Reverse();

        var velocityTrend = await _dbContext.KpiSnapshots
            .AsNoTracking()
            .Where(item => projectIds.Contains(item.ProjectId) && item.SprintVelocity.HasValue)
            .GroupBy(item => item.SnapshotDate.Date)
            .OrderByDescending(group => group.Key)
            .Take(8)
            .Select(group => new ProjectManagerChartPointDto
            {
                Label = group.Key.ToString("dd MMM"),
                Value = decimal.Round(group.Average(item => item.SprintVelocity) ?? 0, 2),
            })
            .ToListAsync(cancellationToken);

        velocityTrend.Reverse();

        return new ProjectManagerKpisDto
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

    public async Task<ProjectSprintGovernanceDto> GetSprintGovernanceAsync(Guid? userId, int projectId, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects.AsNoTracking()
            .Where(item => item.ProjectId == projectId)
            .Select(item => new { item.ProjectId, item.ProjectName })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Project with ID {projectId} was not found.");

        var assignmentQuery = _dbContext.ProjectManagerAssignments.AsNoTracking()
            .Where(item => item.ProjectId == projectId);
        if (userId.HasValue)
        {
            assignmentQuery = assignmentQuery.Where(item => item.ProjectManagerUserId == userId.Value);
        }

        var assignment = await assignmentQuery
            .OrderByDescending(item => item.AssignedAtUtc)
            .Select(item => item.DeliveryHealth)
            .FirstOrDefaultAsync(cancellationToken);

        var sprint = await _dbContext.Sprints.AsNoTracking()
            .Where(item => item.ProjectId == projectId)
            .OrderBy(item => item.Status == "Active" || item.Status == "Current" ? 0 : item.Status == "Planned" ? 1 : 2)
            .ThenByDescending(item => item.EndDate)
            .Select(item => new { item.SprintId, item.SprintName, item.StartDate, item.EndDate, item.PlannedStoryPoints, item.CompletedStoryPoints, item.TotalWorkItems, item.CompletedWorkItems })
            .FirstOrDefaultAsync(cancellationToken);

        var workItemsQuery = _dbContext.WorkItems.AsNoTracking().Where(item => item.ProjectId == projectId);
        if (sprint is not null)
        {
            workItemsQuery = workItemsQuery.Where(item => item.SprintId == sprint.SprintId);
        }

        var workItems = await workItemsQuery
            .OrderByDescending(item => item.IsBlocked)
            .ThenByDescending(item => item.LastUpdatedUtc)
            .Select(item => new { item.WorkItemId, item.AzureWorkItemId, item.Title, item.WorkItemType, item.State, item.Priority, item.AssignedToName, item.StoryPoints, item.ProgressPercent, item.IsBlocked, item.LastUpdatedUtc })
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var isPastDue = sprint is not null && sprint.EndDate < now;
        var plannedItems = sprint?.TotalWorkItems ?? workItems.Count;
        var completedItems = sprint?.CompletedWorkItems ?? workItems.Count(item => IsCompletedState(item.State));
        var plannedPoints = (decimal)(sprint?.PlannedStoryPoints ?? workItems.Sum(item => item.StoryPoints ?? 0));
        var completedPoints = (decimal)(sprint?.CompletedStoryPoints ?? workItems.Where(item => IsCompletedState(item.State)).Sum(item => item.StoryPoints ?? 0));
        var capacityUsed = CalculatePercentage(completedPoints, plannedPoints, completedItems, plannedItems);
        var latestCompletionRate = await _dbContext.KpiSnapshots.AsNoTracking()
            .Where(item => item.ProjectId == projectId)
            .OrderByDescending(item => item.SnapshotDate)
            .Select(item => item.CompletionRate)
            .FirstOrDefaultAsync(cancellationToken);

        var board = new[] { "To Do", "In Progress", "In Review", "Done" }
            .Select(name => new ProjectSprintBoardColumnDto { Name = name })
            .ToList();
        foreach (var item in workItems)
        {
            var completed = IsCompletedState(item.State);
            var delayed = !completed && (item.IsBlocked || isPastDue);
            board.First(column => column.Name == ToBoardColumn(item.State)).Items.Add(new ProjectSprintBoardItemDto
            {
                WorkItemId = item.WorkItemId, AzureWorkItemId = item.AzureWorkItemId, Title = item.Title,
                WorkItemType = item.WorkItemType, State = item.State, Priority = item.Priority,
                AssignedToName = item.AssignedToName, StoryPoints = item.StoryPoints,
                ProgressPercent = item.ProgressPercent, IsBlocked = item.IsBlocked, IsDelayed = delayed,
                LastUpdatedUtc = item.LastUpdatedUtc,
            });
        }

        return new ProjectSprintGovernanceDto
        {
            ProjectId = project.ProjectId, ProjectName = project.ProjectName,
            DeliveryHealth = string.IsNullOrWhiteSpace(assignment) ? "Not assessed" : assignment,
            SprintId = sprint?.SprintId ?? 0, SprintName = sprint?.SprintName ?? "No active sprint",
            SprintStartDate = sprint?.StartDate, SprintEndDate = sprint?.EndDate,
            SprintHealth = decimal.Round(latestCompletionRate ?? capacityUsed, 2),
            CapacityUsedPercent = decimal.Round(capacityUsed, 2),
            CompletedStoryPoints = completedPoints, PlannedStoryPoints = plannedPoints,
            CompletedItems = completedItems, PlannedItems = plannedItems,
            DelayedItems = workItems.Count(item => !IsCompletedState(item.State) && (item.IsBlocked || isPastDue)),
            GeneratedAtUtc = now, Board = board,
        };
    }

    private Task<List<int>> GetAssignedProjectIdsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.ProjectManagerAssignments
            .AsNoTracking()
            .Where(item => item.ProjectManagerUserId == userId)
            .Select(item => item.ProjectId)
            .Distinct()
            .ToListAsync(cancellationToken);
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

    private static string ToBoardColumn(string state)
    {
        if (IsCompletedState(state)) return "Done";
        if (state.Equals("In Review", StringComparison.OrdinalIgnoreCase) || state.Equals("Resolved", StringComparison.OrdinalIgnoreCase) || state.Equals("Ready for Review", StringComparison.OrdinalIgnoreCase)) return "In Review";
        if (state.Equals("In Progress", StringComparison.OrdinalIgnoreCase) || state.Equals("Active", StringComparison.OrdinalIgnoreCase) || state.Equals("Blocked", StringComparison.OrdinalIgnoreCase)) return "In Progress";
        return "To Do";
    }

    private static bool IsCompletedState(string state) =>
        state.Equals("Done", StringComparison.OrdinalIgnoreCase)
        || state.Equals("Closed", StringComparison.OrdinalIgnoreCase)
        || state.Equals("Completed", StringComparison.OrdinalIgnoreCase);

    private static decimal CalculatePercentage(decimal completedPoints, decimal plannedPoints, int completedItems, int plannedItems)
    {
        if (plannedPoints > 0) return completedPoints / plannedPoints * 100;
        return plannedItems > 0 ? (decimal)completedItems / plannedItems * 100 : 0;
    }
}
