using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Sarathi.API.Application.DTOs.Admin;
using Sarathi.API.Application.DTOs.ProjectManager;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Domain.Entities;
using Sarathi.API.Persistence;

namespace Sarathi.API.Application.Services;

public class AdminService : IAdminService
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IAdminProjectStatisticsRepository _adminProjectStatisticsRepository;
    private readonly IProjectManagerRepository _projectManagerRepository;
    private readonly AppDbContext _dbContext;

    public AdminService(
        IAdminConfigurationRepository adminConfigurationRepository,
        IAdminProjectStatisticsRepository adminProjectStatisticsRepository,
        IProjectManagerRepository projectManagerRepository,
        AppDbContext dbContext)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _adminProjectStatisticsRepository = adminProjectStatisticsRepository;
        _projectManagerRepository = projectManagerRepository;
        _dbContext = dbContext;
    }

    public async Task<AdminConfigurationDto> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await _adminConfigurationRepository.GetActiveAsync(cancellationToken)
            ?? CreateDefaultConfiguration();

        return ToDto(configuration);
    }

    public async Task<AdminConfigurationDto> UpdateConfigurationAsync(
        UpdateAdminConfigurationRequestDto request,
        Guid updatedByUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var existingConfiguration = await _adminConfigurationRepository.GetActiveAsync(cancellationToken);

        var entity = new AdminConfiguration
        {
            KpiRefreshIntervalMinutes = request.KpiRefreshIntervalMinutes,
            SprintVelocityTarget = request.SprintVelocityTarget,
            CompletionRateTarget = request.CompletionRateTarget,
            DefectDensityThreshold = request.DefectDensityThreshold,
            BacklogHealthThreshold = request.BacklogHealthThreshold,
            ReleaseSuccessRateTarget = request.ReleaseSuccessRateTarget,
            AzureDevOpsOrganizationUrl = request.AzureDevOpsOrganizationUrl.Trim(),
            AzureDevOpsProjectFilter = request.AzureDevOpsProjectFilter.Trim(),
            AzureDevOpsSyncEnabled = existingConfiguration?.AzureDevOpsSyncEnabled ?? false,
            AzureDevOpsPatCipherText = existingConfiguration?.AzureDevOpsPatCipherText ?? string.Empty,
            AzureDevOpsPatUpdatedAtUtc = existingConfiguration?.AzureDevOpsPatUpdatedAtUtc,
            UpdatedAtUtc = DateTime.UtcNow,
            UpdatedByUserId = updatedByUserId,
            IsActive = true,
        };

        var updated = await _adminConfigurationRepository.UpsertAsync(entity, cancellationToken);
        return ToDto(updated);
    }

    public Task<AdminProjectStatisticsDto> GetProjectStatisticsAsync(int take, CancellationToken cancellationToken = default)
    {
        var normalizedTake = take <= 0 ? 20 : Math.Min(take, 100);
        return _adminProjectStatisticsRepository.GetProjectStatisticsAsync(normalizedTake, cancellationToken);
    }

    public Task<ProjectSprintGovernanceDto> GetSprintGovernanceAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return _projectManagerRepository.GetSprintGovernanceAsync(null, projectId, cancellationToken);
    }

    public async Task<AdminProjectDetailsDto> GetProjectDetailsAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");
        }

        var riskAnalyses = await TryLoadAsync(
            () => _dbContext.ProjectRiskAnalyses
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId)
                .OrderByDescending(item => item.GeneratedDate)
                .ToListAsync(cancellationToken),
            new List<ProjectRiskAnalysis>(),
            cancellationToken);

        var workItems = await TryLoadAsync(
            () => _dbContext.WorkItems
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId)
                .OrderByDescending(item => item.LastUpdatedUtc)
                .Select(item => new AdminProjectWorkItemDto
                {
                    WorkItemId = item.WorkItemId,
                    AzureWorkItemId = item.AzureWorkItemId,
                    Title = item.Title,
                    WorkItemType = item.WorkItemType,
                    State = item.State,
                    Priority = item.Priority,
                    AssignedToName = item.AssignedToName,
                    StoryPoints = item.StoryPoints,
                    ProgressPercent = item.ProgressPercent,
                    IsBlocked = item.IsBlocked,
                    LastUpdatedUtc = item.LastUpdatedUtc,
                    SprintName = item.Sprint != null ? item.Sprint.SprintName : null,
                })
                .ToListAsync(cancellationToken),
            new List<AdminProjectWorkItemDto>(),
            cancellationToken);

        var sprints = await TryLoadAsync(
            () => _dbContext.Sprints
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId)
                .OrderByDescending(item => item.StartDate)
                .Select(item => new AdminProjectSprintDto
                {
                    SprintId = item.SprintId,
                    AzureIterationId = item.AzureIterationId,
                    SprintName = item.SprintName,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    PlannedStoryPoints = item.PlannedStoryPoints,
                    CompletedStoryPoints = item.CompletedStoryPoints,
                    TotalWorkItems = item.TotalWorkItems,
                    CompletedWorkItems = item.CompletedWorkItems,
                    Status = item.Status,
                })
                .ToListAsync(cancellationToken),
            new List<AdminProjectSprintDto>(),
            cancellationToken);

        var repositories = await TryLoadAsync(
            () => _dbContext.Repositories
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.RepositoryName)
                .Select(item => new AdminProjectRepositoryDto
                {
                    RepositoryId = item.RepositoryId,
                    AzureRepoId = item.AzureRepoId,
                    RepositoryName = item.RepositoryName,
                    DefaultBranch = item.DefaultBranch,
                    Size = item.Size,
                    Url = item.Url,
                    LastUpdatedUtc = item.LastUpdatedUtc,
                })
                .ToListAsync(cancellationToken),
            new List<AdminProjectRepositoryDto>(),
            cancellationToken);

        var builds = await TryLoadAsync(
            () => _dbContext.Builds
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId)
                .OrderByDescending(item => item.StartTime)
                .Select(item => new AdminProjectBuildDto
                {
                    BuildId = item.BuildId,
                    AzureBuildId = item.AzureBuildId,
                    DefinitionName = item.DefinitionName,
                    BuildNumber = item.BuildNumber,
                    Status = item.Status,
                    Result = item.Result,
                    SourceBranch = item.SourceBranch,
                    StartTime = item.StartTime,
                    FinishTime = item.FinishTime,
                    TriggerType = item.TriggerType,
                })
                .ToListAsync(cancellationToken),
            new List<AdminProjectBuildDto>(),
            cancellationToken);

        var teamMembers = await TryLoadAsync(
            () => _dbContext.ProjectManagerAssignments
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId)
                .Join(
                    _dbContext.Users.AsNoTracking(),
                    assignment => assignment.ProjectManagerUserId,
                    user => user.UserId,
                    (assignment, user) => new AdminProjectTeamMemberDto
                    {
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role.ToString(),
                        ProjectRole = string.IsNullOrEmpty(assignment.ProjectRole) ? "Not Assigned" : assignment.ProjectRole,
                        AllocationPercent = assignment.AllocationPercent,
                    })
                .ToListAsync(cancellationToken),
            new List<AdminProjectTeamMemberDto>(),
            cancellationToken);

        var latestRisk = riskAnalyses.FirstOrDefault();
        var completedWorkItems = workItems.Count(w => w.State.ToLower().Contains("done") || w.State.ToLower().Contains("closed"));
        var completionRate = workItems.Count > 0
            ? (decimal)completedWorkItems / workItems.Count * 100
            : 0;

        var avgVelocity = sprints.Count > 0
            ? sprints.Average(s => s.CompletedStoryPoints)
            : 0;

        return new AdminProjectDetailsDto
        {
            ProjectId = project.ProjectId,
            ProjectName = project.ProjectName,
            AzureProjectId = project.AzureProjectId,
            Description = project.Description ?? string.Empty,
            Visibility = project.Visibility,
            CreatedDate = project.CreatedDate,
            LastUpdated = project.LastUpdated,
            TotalWorkItems = workItems.Count,
            CompletedWorkItems = completedWorkItems,
            TotalSprints = sprints.Count,
            ActiveSprints = sprints.Count(s => s.Status.ToLower() == "active" || s.Status.ToLower() == "current"),
            AverageCompletionRate = completionRate,
            AverageSprintVelocity = (decimal?)avgVelocity,
            RiskScore = latestRisk?.RiskScore,
            RiskLevel = latestRisk?.RiskLevel,
            WorkItems = workItems,
            Sprints = sprints,
            Repositories = repositories,
            Builds = builds,
            TeamMembers = teamMembers
        };
    }

    private static AdminConfiguration CreateDefaultConfiguration()
    {
        return new AdminConfiguration
        {
            KpiRefreshIntervalMinutes = 60,
            SprintVelocityTarget = 45,
            CompletionRateTarget = 85,
            DefectDensityThreshold = 1.5m,
            BacklogHealthThreshold = 75,
            ReleaseSuccessRateTarget = 90,
            AzureDevOpsOrganizationUrl = string.Empty,
            AzureDevOpsProjectFilter = string.Empty,
            AzureDevOpsSyncEnabled = false,
            AzureDevOpsPatCipherText = string.Empty,
            UpdatedAtUtc = DateTime.UtcNow,
            IsActive = true,
        };
    }

    private static AdminConfigurationDto ToDto(AdminConfiguration entity)
    {
        return new AdminConfigurationDto
        {
            KpiRefreshIntervalMinutes = entity.KpiRefreshIntervalMinutes,
            SprintVelocityTarget = entity.SprintVelocityTarget,
            CompletionRateTarget = entity.CompletionRateTarget,
            DefectDensityThreshold = entity.DefectDensityThreshold,
            BacklogHealthThreshold = entity.BacklogHealthThreshold,
            ReleaseSuccessRateTarget = entity.ReleaseSuccessRateTarget,
            AzureDevOpsOrganizationUrl = entity.AzureDevOpsOrganizationUrl,
            AzureDevOpsProjectFilter = entity.AzureDevOpsProjectFilter,
            UpdatedAtUtc = entity.UpdatedAtUtc,
        };
    }

    private static void ValidateRequest(UpdateAdminConfigurationRequestDto request)
    {
        if (request.KpiRefreshIntervalMinutes < 5 || request.KpiRefreshIntervalMinutes > 1440)
        {
            throw new ArgumentOutOfRangeException(nameof(request.KpiRefreshIntervalMinutes), "KPI refresh interval must be between 5 and 1440 minutes.");
        }

        if (request.CompletionRateTarget is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(request.CompletionRateTarget), "Completion rate target must be between 0 and 100.");
        }

        if (request.BacklogHealthThreshold is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(request.BacklogHealthThreshold), "Backlog health threshold must be between 0 and 100.");
        }

        if (request.ReleaseSuccessRateTarget is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(request.ReleaseSuccessRateTarget), "Release success target must be between 0 and 100.");
        }
    }

    private static async Task<T> TryLoadAsync<T>(Func<Task<T>> loader, T fallback, CancellationToken cancellationToken = default)
    {
        try
        {
            return await loader();
        }
        catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException or SqlException)
        {
            return fallback;
        }
    }
}
