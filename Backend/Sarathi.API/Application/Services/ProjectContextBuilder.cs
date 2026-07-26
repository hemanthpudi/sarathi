using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.DTOs.ProjectManager;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Persistence;

namespace Sarathi.API.Application.Services;

/// <summary>
/// Builds comprehensive project context from synchronized Azure DevOps data.
/// Organizes work items by type, collects team information, and gathers KPIs and risks
/// to provide rich context for AI analysis.
/// </summary>
public class ProjectContextBuilder : IProjectContextBuilder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProjectContextBuilder> _logger;

    public ProjectContextBuilder(AppDbContext dbContext, ILogger<ProjectContextBuilder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ProjectContextDto> BuildProjectContextAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var context = new ProjectContextDto();

        try
        {
            // Get project details
            var project = await _dbContext.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken)
                ?? throw new KeyNotFoundException($"Project with ID {projectId} not found");

            context.Project = new ProjectSummaryDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                Visibility = project.Visibility,
                CreatedDate = project.CreatedDate,
                LastUpdated = project.LastUpdated
            };

            // Get all work items for this project
            var workItems = await _dbContext.WorkItems.AsNoTracking()
                .Where(w => w.ProjectId == projectId)
                .ToListAsync(cancellationToken);

            // Organize work items by type
            var sprintMap = await GetSprintMapAsync(projectId, cancellationToken);
            
            context.Epics = workItems
                .Where(w => w.WorkItemType.Equals("Epic", StringComparison.OrdinalIgnoreCase))
                .Select(w => MapWorkItemToCategory(w, sprintMap))
                .ToList();

            context.Features = workItems
                .Where(w => w.WorkItemType.Equals("Feature", StringComparison.OrdinalIgnoreCase))
                .Select(w => MapWorkItemToCategory(w, sprintMap))
                .ToList();

            context.UserStories = workItems
                .Where(w => w.WorkItemType.Equals("User Story", StringComparison.OrdinalIgnoreCase))
                .Select(w => MapWorkItemToCategory(w, sprintMap))
                .ToList();

            context.Tasks = workItems
                .Where(w => w.WorkItemType.Equals("Task", StringComparison.OrdinalIgnoreCase))
                .Select(w => MapWorkItemToCategory(w, sprintMap))
                .ToList();

            context.Bugs = workItems
                .Where(w => w.WorkItemType.Equals("Bug", StringComparison.OrdinalIgnoreCase))
                .Select(w => MapWorkItemToCategory(w, sprintMap))
                .ToList();

            // Get sprints
            var sprints = await _dbContext.Sprints.AsNoTracking()
                .Where(s => s.ProjectId == projectId)
                .ToListAsync(cancellationToken);

            context.Sprints = sprints.Select(s => new SprintSummaryDto
            {
                SprintId = s.SprintId,
                SprintName = s.SprintName,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                PlannedStoryPoints = s.PlannedStoryPoints,
                CompletedStoryPoints = s.CompletedStoryPoints,
                TotalWorkItems = s.TotalWorkItems,
                CompletedWorkItems = s.CompletedWorkItems,
                Status = s.Status
            }).ToList();

            // Get team members
            context.Team = await BuildTeamAsync(projectId, workItems, cancellationToken);

            // Get latest KPIs
            var latestKpi = await _dbContext.KpiSnapshots.AsNoTracking()
                .Where(k => k.ProjectId == projectId)
                .OrderByDescending(k => k.SnapshotDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestKpi != null)
            {
                context.Kpis = new KpiContextDto
                {
                    SprintVelocity = latestKpi.SprintVelocity,
                    CompletionRate = latestKpi.CompletionRate,
                    DefectDensity = latestKpi.DefectDensity,
                    BacklogHealth = latestKpi.BacklogHealth,
                    ReleaseSuccessRate = latestKpi.ReleaseSuccessRate,
                    SnapshotDate = latestKpi.SnapshotDate
                };
            }

            // Get risks
            var risks = await _dbContext.ProjectRiskAnalyses.AsNoTracking()
                .Where(r => r.ProjectId == projectId)
                .OrderByDescending(r => r.GeneratedDate)
                .Take(5)
                .ToListAsync(cancellationToken);

            context.Risks = risks.Select(r => new RiskSummaryDto
            {
                Id = r.Id,
                RiskScore = r.RiskScore,
                RiskLevel = r.RiskLevel,
                RiskSummary = r.RiskSummary,
                GeneratedDate = r.GeneratedDate
            }).ToList();

            // Get pipelines (builds)
            var builds = await _dbContext.Builds.AsNoTracking()
                .Where(b => b.ProjectId == projectId)
                .OrderByDescending(b => b.StartTime)
                .Take(10)
                .ToListAsync(cancellationToken);

            context.Pipelines = builds.Select(b => new PipelineSummaryDto
            {
                BuildId = b.BuildId,
                DefinitionName = b.DefinitionName,
                BuildNumber = b.BuildNumber,
                Status = b.Status,
                Result = b.Result,
                SourceBranch = b.SourceBranch,
                StartTime = b.StartTime,
                FinishTime = b.FinishTime
            }).ToList();

            // Get repositories
            var repositories = await _dbContext.Repositories.AsNoTracking()
                .Where(r => r.ProjectId == projectId)
                .ToListAsync(cancellationToken);

            context.Repositories = repositories.Select(r => new RepositorySummaryDto
            {
                RepositoryId = r.RepositoryId,
                RepositoryName = r.RepositoryName,
                DefaultBranch = r.DefaultBranch,
                Size = r.Size,
                Url = r.Url
            }).ToList();

            // Get sync status
            var lastSync = await _dbContext.AzureDevOpsSyncJobs.AsNoTracking()
                .OrderByDescending(s => s.CompletedAtUtc ?? s.StartedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            context.SyncStatus = new SynchronizationStatusDto
            {
                LastSyncUtc = lastSync?.CompletedAtUtc ?? lastSync?.StartedAtUtc,
                LastSyncStatus = lastSync?.Status ?? "No sync recorded",
                ItemsProcessedInLastSync = lastSync?.ItemsProcessed,
                ItemsSucceededInLastSync = lastSync?.ItemsSucceeded,
                ItemsFailedInLastSync = lastSync?.ItemsFailed,
                LastSyncErrorMessage = lastSync?.ErrorMessage,
                LastSyncDurationSeconds = lastSync?.DurationSeconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building project context for project {ProjectId}", projectId);
            throw;
        }

        return context;
    }

    private WorkItemCategoryDto MapWorkItemToCategory(Domain.Entities.WorkItem workItem, Dictionary<int, string> sprintMap)
    {
        var sprintName = workItem.SprintId.HasValue && sprintMap.TryGetValue(workItem.SprintId.Value, out var name) ? name : null;

        return new WorkItemCategoryDto
        {
            WorkItemId = workItem.WorkItemId,
            Title = workItem.Title,
            State = workItem.State,
            Priority = workItem.Priority,
            AssignedTo = workItem.AssignedToName,
            StoryPoints = workItem.StoryPoints,
            ProgressPercent = workItem.ProgressPercent,
            IsBlocked = workItem.IsBlocked,
            LastUpdated = workItem.LastUpdatedUtc,
            SprintName = sprintName
        };
    }

    private async Task<Dictionary<int, string>> GetSprintMapAsync(int projectId, CancellationToken cancellationToken)
    {
        var sprints = await _dbContext.Sprints.AsNoTracking()
            .Where(s => s.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        return sprints.ToDictionary(s => s.SprintId, s => s.SprintName);
    }

    private async Task<List<TeamMemberDto>> BuildTeamAsync(
        int projectId,
        List<Domain.Entities.WorkItem> workItems,
        CancellationToken cancellationToken)
    {
        var teamMembers = new Dictionary<string, TeamMemberDto>();

        // Add project managers
        var projectManagers = await _dbContext.ProjectManagerAssignments.AsNoTracking()
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        foreach (var pm in projectManagers)
        {
            var key = pm.Id.ToString(); // Use assignment ID as temporary key
            if (!teamMembers.ContainsKey(key))
            {
                teamMembers[key] = new TeamMemberDto
                {
                    Name = $"Project Manager (ID: {pm.ProjectManagerUserId})",
                    Role = pm.ProjectRole,
                    AllocationPercent = pm.AllocationPercent,
                    DeliveryHealth = pm.DeliveryHealth,
                    Source = "ProjectManager"
                };
            }
        }

        // Add team members from work item assignments (unique)
        var assignedMembers = workItems
            .Where(w => !string.IsNullOrWhiteSpace(w.AssignedToName))
            .GroupBy(w => w.AssignedToName)
            .Select(g => new TeamMemberDto
            {
                Name = g.Key,
                Role = "Team Member",
                AllocationPercent = 100, // Default allocation
                DeliveryHealth = "On Track",
                Source = "WorkItemAssignment"
            });

        foreach (var member in assignedMembers)
        {
            if (!teamMembers.ContainsKey(member.Name))
            {
                teamMembers[member.Name] = member;
            }
        }

        return teamMembers.Values.ToList();
    }
}
