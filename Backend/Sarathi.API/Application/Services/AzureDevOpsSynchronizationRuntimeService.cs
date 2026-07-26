using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Domain.Entities;
using Sarathi.API.Persistence;

namespace Sarathi.API.Application.Services;

public class AzureDevOpsSynchronizationRuntimeService : IAzureDevOpsSynchronizationRuntimeService
{
    private readonly AppDbContext _dbContext;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IAzureDevOpsIntegrationRepository _azureDevOpsIntegrationRepository;
    private readonly IAzureDevOpsClient _azureDevOpsClient;
    private readonly IDataProtector _protector;
    private readonly string? _configuredPersonalAccessToken;
    private readonly ILogger<AzureDevOpsSynchronizationRuntimeService> _logger;

    public AzureDevOpsSynchronizationRuntimeService(
        AppDbContext dbContext,
        IAdminConfigurationRepository adminConfigurationRepository,
        IAzureDevOpsIntegrationRepository azureDevOpsIntegrationRepository,
        IAzureDevOpsClient azureDevOpsClient,
        IDataProtectionProvider dataProtectionProvider,
        IConfiguration configuration,
        ILogger<AzureDevOpsSynchronizationRuntimeService> logger)
    {
        _dbContext = dbContext;
        _adminConfigurationRepository = adminConfigurationRepository;
        _azureDevOpsIntegrationRepository = azureDevOpsIntegrationRepository;
        _azureDevOpsClient = azureDevOpsClient;
        _protector = dataProtectionProvider.CreateProtector("Sarathi.API.AzureDevOps.PAT");
        _configuredPersonalAccessToken = configuration["AzureDevOps:PersonalAccessToken"];
        _logger = logger;
    }

    public async Task RunCycleAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var dueSchedules = await _azureDevOpsIntegrationRepository.GetDueSchedulesAsync(utcNow, cancellationToken);
        foreach (var schedule in dueSchedules)
        {
            await _azureDevOpsIntegrationRepository.QueueScheduleJobAsync(schedule, cancellationToken);
        }

        var queuedJobs = await _azureDevOpsIntegrationRepository.GetQueuedJobsAsync(3, cancellationToken);
        foreach (var job in queuedJobs)
        {
            await ProcessJobAsync(job, cancellationToken);
        }
    }

    public async Task<bool> SynchronizeNowAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await _adminConfigurationRepository.GetActiveAsync(cancellationToken);
        if (configuration is null || !configuration.AzureDevOpsSyncEnabled)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(configuration.AzureDevOpsOrganizationUrl)
            || (string.IsNullOrWhiteSpace(configuration.AzureDevOpsPatCipherText) && string.IsNullOrWhiteSpace(_configuredPersonalAccessToken)))
        {
            return false;
        }

        var job = new AzureDevOpsSyncJob
        {
            SyncType = "Full",
            ScopeName = "All Projects",
            Status = "Queued",
            Source = "OnDemand",
            TriggeredByDisplayName = "LiveFirstSync",
            StartedAtUtc = DateTime.UtcNow,
        };

        _dbContext.AzureDevOpsSyncJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await ProcessJobAsync(job, cancellationToken);

        return job.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
    }

    private async Task ProcessJobAsync(AzureDevOpsSyncJob job, CancellationToken cancellationToken)
    {
        var startedAt = DateTime.UtcNow;

        try
        {
            var trackedJob = await _azureDevOpsIntegrationRepository.GetJobAsync(job.Id, cancellationToken);
            if (trackedJob is null)
            {
                return;
            }

            var configuration = await _adminConfigurationRepository.GetActiveAsync(cancellationToken);
            if (configuration is null || (string.IsNullOrWhiteSpace(configuration.AzureDevOpsOrganizationUrl)
                || (string.IsNullOrWhiteSpace(configuration.AzureDevOpsPatCipherText) && string.IsNullOrWhiteSpace(_configuredPersonalAccessToken))))
            {
                await FailJobAsync(trackedJob, "Azure DevOps organization URL or PAT is not configured.", cancellationToken);
                return;
            }

            trackedJob.Status = "Running";
            trackedJob.ErrorMessage = null;
            trackedJob.StartedAtUtc = startedAt;
            await _azureDevOpsIntegrationRepository.SaveChangesAsync(cancellationToken);

            var personalAccessToken = !string.IsNullOrWhiteSpace(configuration.AzureDevOpsPatCipherText)
                ? _protector.Unprotect(configuration.AzureDevOpsPatCipherText)
                : _configuredPersonalAccessToken!;
            var syncedProjects = await SyncAsync(configuration, trackedJob, personalAccessToken, cancellationToken);

            trackedJob.Status = "Completed";
            trackedJob.CompletedAtUtc = DateTime.UtcNow;
            trackedJob.DurationSeconds = (int)Math.Max(0, (trackedJob.CompletedAtUtc.Value - startedAt).TotalSeconds);
            trackedJob.ItemsSucceeded = trackedJob.ItemsProcessed;
            trackedJob.ItemsFailed = 0;

            await _azureDevOpsIntegrationRepository.LogAsync(
                "Information",
                "AzureDevOpsSynchronization",
                $"{trackedJob.SyncType} synchronization completed for {trackedJob.ScopeName}. Synced {syncedProjects} project scope(s).",
                "Azure DevOps Integration",
                trackedJob.Id.ToString(),
                cancellationToken);

            await _azureDevOpsIntegrationRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure DevOps synchronization job {JobId} failed.", job.Id);

            var trackedJob = await _azureDevOpsIntegrationRepository.GetJobAsync(job.Id, cancellationToken);
            if (trackedJob is not null)
            {
                trackedJob.Status = "Failed";
                trackedJob.ErrorMessage = ex.Message;
                trackedJob.CompletedAtUtc = DateTime.UtcNow;
                trackedJob.DurationSeconds = (int)Math.Max(0, (trackedJob.CompletedAtUtc.Value - startedAt).TotalSeconds);

                await _azureDevOpsIntegrationRepository.LogAsync(
                    "Error",
                    "AzureDevOpsSynchronization",
                    $"Synchronization job {trackedJob.Id} failed: {ex.Message}",
                    "Azure DevOps Integration",
                    trackedJob.Id.ToString(),
                    cancellationToken);

                await _azureDevOpsIntegrationRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task<int> SyncAsync(AdminConfiguration configuration, AzureDevOpsSyncJob job, string personalAccessToken, CancellationToken cancellationToken)
    {
        var projectFilter = ResolveProjectFilter(configuration.AzureDevOpsProjectFilter, job.ScopeName);
        var syncType = job.SyncType.Trim();
        var processedProjects = 0;

        var projects = await SyncProjectsAsync(configuration.AzureDevOpsOrganizationUrl, personalAccessToken, projectFilter, cancellationToken);
        processedProjects += projects.Count;

        if (IsFullOrMatches(syncType, "Sprints") || IsFullOrMatches(syncType, "Iterations"))
        {
            job.ItemsProcessed += await SyncSprintsAsync(configuration.AzureDevOpsOrganizationUrl, personalAccessToken, projects, cancellationToken);
        }

        if (IsFullOrMatches(syncType, "WorkItems") || IsFullOrMatches(syncType, "Work Item"))
        {
            job.ItemsProcessed += await SyncWorkItemsAsync(configuration.AzureDevOpsOrganizationUrl, personalAccessToken, projects, cancellationToken);
        }

        if (IsFullOrMatches(syncType, "Repositories"))
        {
            job.ItemsProcessed += await SyncRepositoriesAsync(configuration.AzureDevOpsOrganizationUrl, personalAccessToken, projects, cancellationToken);
        }

        if (IsFullOrMatches(syncType, "Pipelines") || IsFullOrMatches(syncType, "Builds"))
        {
            job.ItemsProcessed += await SyncBuildsAsync(configuration.AzureDevOpsOrganizationUrl, personalAccessToken, projects, cancellationToken);
        }

        if (IsFullOrMatches(syncType, "Projects"))
        {
            job.ItemsProcessed += projects.Count;
        }

        job.ItemsProcessed += await RefreshDerivedAnalyticsAsync(configuration, configuration.AzureDevOpsOrganizationUrl, personalAccessToken, projects, cancellationToken);

        return processedProjects;
    }

    private async Task<int> SyncRepositoriesAsync(string organizationUrl, string personalAccessToken, IReadOnlyList<Project> projects, CancellationToken cancellationToken)
    {
        var processed = 0;

        foreach (var project in projects)
        {
            try
            {
                var repositoryData = await _azureDevOpsClient.GetRepositoriesAsync(organizationUrl, project.ProjectName, personalAccessToken, cancellationToken);
                var repositoryIds = repositoryData.Select(item => item.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var existingRepositories = await _dbContext.Repositories
                    .Where(item => item.ProjectId == project.ProjectId && repositoryIds.Contains(item.AzureRepoId))
                    .ToListAsync(cancellationToken);

                foreach (var data in repositoryData)
                {
                    var repository = existingRepositories.FirstOrDefault(item => item.AzureRepoId == data.Id);
                    if (repository is null)
                    {
                        repository = new Repository
                        {
                            ProjectId = project.ProjectId,
                            AzureRepoId = data.Id,
                        };
                        _dbContext.Repositories.Add(repository);
                        existingRepositories.Add(repository);
                    }

                    repository.RepositoryName = data.Name;
                    repository.DefaultBranch = data.DefaultBranch;
                    repository.Size = data.Size;
                    repository.Url = data.Url;
                    repository.LastUpdatedUtc = DateTime.UtcNow;
                    processed++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Repository synchronization failed for project {ProjectName}.", project.ProjectName);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return processed;
    }

    private async Task<int> SyncBuildsAsync(string organizationUrl, string personalAccessToken, IReadOnlyList<Project> projects, CancellationToken cancellationToken)
    {
        var processed = 0;

        foreach (var project in projects)
        {
            try
            {
                var buildData = await _azureDevOpsClient.GetBuildsAsync(organizationUrl, project.ProjectName, personalAccessToken, cancellationToken);
                var buildIds = buildData.Select(item => item.Id).ToHashSet();
                var existingBuilds = await _dbContext.Builds
                    .Where(item => item.ProjectId == project.ProjectId && buildIds.Contains(item.AzureBuildId))
                    .ToListAsync(cancellationToken);

                foreach (var data in buildData)
                {
                    var build = existingBuilds.FirstOrDefault(item => item.AzureBuildId == data.Id);
                    if (build is null)
                    {
                        build = new Build
                        {
                            ProjectId = project.ProjectId,
                            AzureBuildId = data.Id,
                        };
                        _dbContext.Builds.Add(build);
                        existingBuilds.Add(build);
                    }

                    build.DefinitionName = data.DefinitionName;
                    build.BuildNumber = data.BuildNumber;
                    build.Status = data.Status;
                    build.Result = string.IsNullOrWhiteSpace(data.Result) ? null : data.Result;
                    build.SourceBranch = data.SourceBranch;
                    build.StartTime = data.StartTimeUtc ?? DateTime.UtcNow;
                    build.FinishTime = data.FinishTimeUtc;
                    build.TriggerType = data.TriggerType;
                    processed++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Pipeline synchronization failed for project {ProjectName}.", project.ProjectName);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return processed;
    }

    private async Task<List<Project>> SyncProjectsAsync(string organizationUrl, string personalAccessToken, HashSet<string> projectFilter, CancellationToken cancellationToken)
    {
        var remoteProjects = await _azureDevOpsClient.GetProjectsAsync(organizationUrl, personalAccessToken, cancellationToken);
        var candidates = remoteProjects
            .Where(item => projectFilter.Count == 0 || projectFilter.Contains(item.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var remoteIds = candidates.Select(item => item.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existing = await _dbContext.Projects
            .Where(item => remoteIds.Contains(item.AzureProjectId))
            .ToListAsync(cancellationToken);

        foreach (var remoteProject in candidates)
        {
            var entity = existing.FirstOrDefault(item => item.AzureProjectId == remoteProject.Id);
            if (entity is null)
            {
                entity = new Project
                {
                    AzureProjectId = remoteProject.Id,
                    ProjectName = remoteProject.Name,
                    Description = remoteProject.Description,
                    Visibility = NormalizeVisibility(remoteProject.Visibility),
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                };
                _dbContext.Projects.Add(entity);
                existing.Add(entity);
            }
            else
            {
                entity.ProjectName = remoteProject.Name;
                entity.Description = remoteProject.Description;
                entity.Visibility = NormalizeVisibility(remoteProject.Visibility);
                entity.LastUpdated = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing.Where(item => remoteIds.Contains(item.AzureProjectId)).ToList();
    }

    private async Task<int> SyncSprintsAsync(string organizationUrl, string personalAccessToken, IReadOnlyList<Project> projects, CancellationToken cancellationToken)
    {
        var processed = 0;

        foreach (var project in projects)
        {
            var iterations = await _azureDevOpsClient.GetSprintsAsync(organizationUrl, project.AzureProjectId, project.ProjectName, personalAccessToken, cancellationToken);
            var iterationIds = iterations.Select(item => item.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var existingSprints = await _dbContext.Sprints
                .Where(item => item.ProjectId == project.ProjectId && iterationIds.Contains(item.AzureIterationId))
                .ToListAsync(cancellationToken);

            foreach (var iteration in iterations)
            {
                var sprint = existingSprints.FirstOrDefault(item => item.AzureIterationId == iteration.Id);
                if (sprint is null)
                {
                    sprint = new Sprint
                    {
                        ProjectId = project.ProjectId,
                        AzureIterationId = iteration.Id,
                        SprintName = iteration.Name,
                    };
                    _dbContext.Sprints.Add(sprint);
                    existingSprints.Add(sprint);
                }

                sprint.SprintName = iteration.Name;
                sprint.StartDate = iteration.StartDate;
                sprint.EndDate = iteration.EndDate;
                sprint.Status = iteration.Status;
                processed++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return processed;
    }

    private async Task<int> SyncWorkItemsAsync(string organizationUrl, string personalAccessToken, IReadOnlyList<Project> projects, CancellationToken cancellationToken)
    {
        var processed = 0;

        foreach (var project in projects)
        {
            var workItems = await _azureDevOpsClient.GetWorkItemsAsync(organizationUrl, project.ProjectName, personalAccessToken, cancellationToken);
            var workItemIds = workItems.Select(item => item.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var existingWorkItems = await _dbContext.WorkItems
                .Where(item => item.ProjectId == project.ProjectId && workItemIds.Contains(item.AzureWorkItemId))
                .ToListAsync(cancellationToken);

            var sprintMap = await _dbContext.Sprints
                .Where(item => item.ProjectId == project.ProjectId)
                .ToDictionaryAsync(item => item.SprintName, StringComparer.OrdinalIgnoreCase, cancellationToken);

            foreach (var workItemData in workItems)
            {
                var workItem = existingWorkItems.FirstOrDefault(item => item.AzureWorkItemId == workItemData.Id);
                if (workItem is null)
                {
                    workItem = new WorkItem
                    {
                        ProjectId = project.ProjectId,
                        AzureWorkItemId = workItemData.Id,
                    };
                    _dbContext.WorkItems.Add(workItem);
                    existingWorkItems.Add(workItem);
                }

                workItem.Title = workItemData.Title;
                workItem.WorkItemType = workItemData.WorkItemType;
                workItem.State = workItemData.State;
                workItem.Priority = workItemData.Priority;
                workItem.AssignedToName = workItemData.AssignedToName;
                workItem.StoryPoints = workItemData.StoryPoints;
                workItem.IsBlocked = workItemData.IsBlocked;
                workItem.ProgressPercent = CalculateProgress(workItemData.State, workItemData.IsBlocked);
                workItem.LastUpdatedUtc = workItemData.LastUpdatedUtc == default ? DateTime.UtcNow : workItemData.LastUpdatedUtc;
                workItem.SprintId = sprintMap.TryGetValue(workItemData.SprintName, out var sprint) ? sprint.SprintId : null;
                processed++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return processed;
    }

    private async Task<int> RefreshDerivedAnalyticsAsync(
        AdminConfiguration configuration,
        string organizationUrl,
        string personalAccessToken,
        IReadOnlyList<Project> projects,
        CancellationToken cancellationToken)
    {
        var processed = 0;
        var snapshotDate = DateTime.UtcNow.Date;

        foreach (var project in projects)
        {
            var sprints = await _dbContext.Sprints
                .Where(item => item.ProjectId == project.ProjectId)
                .OrderBy(item => item.EndDate)
                .ToListAsync(cancellationToken);

            var workItems = await _dbContext.WorkItems
                .Where(item => item.ProjectId == project.ProjectId)
                .ToListAsync(cancellationToken);

            var builds = await GetBuildsSafeAsync(organizationUrl, project.ProjectName, personalAccessToken, cancellationToken);
            var releases = await GetReleasesSafeAsync(organizationUrl, project.ProjectName, personalAccessToken, cancellationToken);

            foreach (var sprint in sprints)
            {
                var sprintWorkItems = workItems.Where(item => item.SprintId == sprint.SprintId).ToList();
                sprint.TotalWorkItems = sprintWorkItems.Count;
                sprint.CompletedWorkItems = sprintWorkItems.Count(item => IsCompletedState(item.State));
                sprint.PlannedStoryPoints = RoundToInt(sprintWorkItems.Sum(item => item.StoryPoints ?? 0));
                sprint.CompletedStoryPoints = RoundToInt(sprintWorkItems.Where(item => IsCompletedState(item.State)).Sum(item => item.StoryPoints ?? 0));
            }

            var referenceSprint = sprints
                .OrderBy(item => item.Status == "Active" ? 0 : item.Status == "Planned" ? 1 : 2)
                .ThenByDescending(item => item.EndDate)
                .FirstOrDefault();

            var totalWorkItems = workItems.Count;
            var completedWorkItems = workItems.Count(item => IsCompletedState(item.State));
            var totalStoryPoints = workItems.Sum(item => item.StoryPoints ?? 0);
            var completedStoryPoints = workItems.Where(item => IsCompletedState(item.State)).Sum(item => item.StoryPoints ?? 0);
            var sprintVelocity = referenceSprint is null ? completedStoryPoints : referenceSprint.CompletedStoryPoints;
            var completionRate = referenceSprint is null
                ? CalculateCompletionRate(totalStoryPoints, completedStoryPoints, totalWorkItems, completedWorkItems)
                : CalculateCompletionRate(referenceSprint.PlannedStoryPoints, referenceSprint.CompletedStoryPoints, referenceSprint.TotalWorkItems, referenceSprint.CompletedWorkItems);

            var bugs = workItems.Where(item => item.WorkItemType.Equals("Bug", StringComparison.OrdinalIgnoreCase)).ToList();
            var openBugs = bugs.Count(item => !IsCompletedState(item.State));
            var defectDensity = workItems.Count == 0
                ? 0
                : decimal.Round((decimal)openBugs / Math.Max(workItems.Count(item => !item.WorkItemType.Equals("Bug", StringComparison.OrdinalIgnoreCase)), 1), 2);

            var backlogCandidates = workItems.Where(item => !IsCompletedState(item.State)).ToList();
            var backlogHealth = backlogCandidates.Count == 0
                ? 100
                : decimal.Round((decimal)backlogCandidates.Count(item => item.StoryPoints.HasValue && item.StoryPoints.Value > 0) / backlogCandidates.Count * 100, 2);

            var completedBuilds = builds.Where(item => item.Status.Equals("completed", StringComparison.OrdinalIgnoreCase)).ToList();
            var successfulBuilds = completedBuilds.Count(item => item.Result.Equals("succeeded", StringComparison.OrdinalIgnoreCase) || item.Result.Equals("partiallySucceeded", StringComparison.OrdinalIgnoreCase));
            var buildSuccessRate = completedBuilds.Count == 0
                ? 0
                : decimal.Round((decimal)successfulBuilds / completedBuilds.Count * 100, 2);

            var completedReleases = releases.Where(item => item.Status.Equals("active", StringComparison.OrdinalIgnoreCase)
                                                           || item.Status.Equals("abandoned", StringComparison.OrdinalIgnoreCase)
                                                           || item.Status.Equals("draft", StringComparison.OrdinalIgnoreCase)
                                                           || item.Status.Equals("undefined", StringComparison.OrdinalIgnoreCase) == false)
                .ToList();
            var successfulReleases = completedReleases.Count(item => item.Status.Equals("active", StringComparison.OrdinalIgnoreCase)
                                                                    || item.Status.Equals("succeeded", StringComparison.OrdinalIgnoreCase));
            var releaseSuccessRate = completedReleases.Count == 0
                ? buildSuccessRate
                : decimal.Round((decimal)successfulReleases / completedReleases.Count * 100, 2);

            var latestSnapshot = await _dbContext.KpiSnapshots
                .FirstOrDefaultAsync(item => item.ProjectId == project.ProjectId && item.SnapshotDate.Date == snapshotDate, cancellationToken);

            if (latestSnapshot is null)
            {
                latestSnapshot = new KpiSnapshot
                {
                    ProjectId = project.ProjectId,
                    SnapshotDate = snapshotDate,
                };
                _dbContext.KpiSnapshots.Add(latestSnapshot);
            }

            latestSnapshot.SprintVelocity = decimal.Round(sprintVelocity, 2);
            latestSnapshot.CompletionRate = completionRate;
            latestSnapshot.DefectDensity = defectDensity;
            latestSnapshot.BacklogHealth = backlogHealth;
            latestSnapshot.ReleaseSuccessRate = releaseSuccessRate;

            var blockedRatio = totalWorkItems == 0
                ? 0
                : decimal.Round((decimal)workItems.Count(item => item.IsBlocked || item.State.Equals("Blocked", StringComparison.OrdinalIgnoreCase)) / totalWorkItems * 100, 2);
            var velocityRisk = configuration.SprintVelocityTarget <= 0
                ? 0
                : ClampPercent(100 - (sprintVelocity / configuration.SprintVelocityTarget) * 100);
            var completionRisk = ClampPercent(100 - completionRate);
            var backlogRisk = ClampPercent(100 - backlogHealth);
            var defectRisk = ClampPercent(configuration.DefectDensityThreshold <= 0
                ? defectDensity * 25
                : defectDensity / configuration.DefectDensityThreshold * 100);
            var releaseRisk = releaseSuccessRate <= 0 ? 40 : ClampPercent(100 - releaseSuccessRate);

            var riskScore = decimal.Round(
                (completionRisk * 0.25m)
                + (velocityRisk * 0.20m)
                + (blockedRatio * 0.15m)
                + (defectRisk * 0.15m)
                + (backlogRisk * 0.15m)
                + (releaseRisk * 0.10m),
                2);

            var latestRisk = await _dbContext.ProjectRiskAnalyses
                .FirstOrDefaultAsync(item => item.ProjectId == project.ProjectId && item.GeneratedDate.Date == snapshotDate, cancellationToken);

            if (latestRisk is null)
            {
                latestRisk = new ProjectRiskAnalysis
                {
                    ProjectId = project.ProjectId,
                };
                _dbContext.ProjectRiskAnalyses.Add(latestRisk);
            }

            latestRisk.GeneratedDate = DateTime.UtcNow;
            latestRisk.RiskScore = riskScore;
            latestRisk.RiskLevel = GetRiskLevel(riskScore);
            latestRisk.RiskSummary = BuildRiskSummary(referenceSprint, blockedRatio, openBugs, releaseSuccessRate, riskScore);

            project.LastUpdated = DateTime.UtcNow;
            processed += 2;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return processed;
    }

    private async Task FailJobAsync(AzureDevOpsSyncJob job, string message, CancellationToken cancellationToken)
    {
        job.Status = "Failed";
        job.ErrorMessage = message;
        job.CompletedAtUtc = DateTime.UtcNow;

        await _azureDevOpsIntegrationRepository.LogAsync(
            "Error",
            "AzureDevOpsSynchronization",
            message,
            "Azure DevOps Integration",
            job.Id.ToString(),
            cancellationToken);

        await _azureDevOpsIntegrationRepository.SaveChangesAsync(cancellationToken);
    }

    private static HashSet<string> ResolveProjectFilter(string configurationFilter, string scopeName)
    {
        if (!string.IsNullOrWhiteSpace(scopeName) && !scopeName.Equals("All Projects", StringComparison.OrdinalIgnoreCase))
        {
            return scopeName.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return configurationFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsFullOrMatches(string syncType, string match)
    {
        return syncType.Equals("Full", StringComparison.OrdinalIgnoreCase)
               || syncType.Equals(match, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeVisibility(string visibility)
    {
        return string.Equals(visibility, "public", StringComparison.OrdinalIgnoreCase) ? "Public" : "Private";
    }

    private static int CalculateProgress(string state, bool isBlocked)
    {
        if (isBlocked || state.Equals("Blocked", StringComparison.OrdinalIgnoreCase))
        {
            return 35;
        }

        if (state.Equals("Done", StringComparison.OrdinalIgnoreCase) || state.Equals("Closed", StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        if (state.Equals("In Progress", StringComparison.OrdinalIgnoreCase) || state.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            return 60;
        }

        return 10;
    }

    private async Task<IReadOnlyList<AzureDevOpsBuildData>> GetBuildsSafeAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken)
    {
        try
        {
            return await _azureDevOpsClient.GetBuildsAsync(organizationUrl, projectName, personalAccessToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Build synchronization metrics could not be retrieved for project {ProjectName}.", projectName);
            return Array.Empty<AzureDevOpsBuildData>();
        }
    }

    private async Task<IReadOnlyList<AzureDevOpsReleaseData>> GetReleasesSafeAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken)
    {
        try
        {
            return await _azureDevOpsClient.GetReleasesAsync(organizationUrl, projectName, personalAccessToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Release synchronization metrics could not be retrieved for project {ProjectName}.", projectName);
            return Array.Empty<AzureDevOpsReleaseData>();
        }
    }

    private static bool IsCompletedState(string state)
    {
        return state.Equals("Done", StringComparison.OrdinalIgnoreCase)
               || state.Equals("Closed", StringComparison.OrdinalIgnoreCase)
               || state.Equals("Completed", StringComparison.OrdinalIgnoreCase)
               || state.Equals("Resolved", StringComparison.OrdinalIgnoreCase);
    }

    private static decimal CalculateCompletionRate(decimal plannedStoryPoints, decimal completedStoryPoints, int totalWorkItems, int completedWorkItems)
    {
        if (plannedStoryPoints > 0)
        {
            return decimal.Round(completedStoryPoints / plannedStoryPoints * 100, 2);
        }

        if (totalWorkItems > 0)
        {
            return decimal.Round((decimal)completedWorkItems / totalWorkItems * 100, 2);
        }

        return 0;
    }

    private static int RoundToInt(decimal value)
    {
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    private static decimal ClampPercent(decimal value)
    {
        return decimal.Clamp(value, 0, 100);
    }

    private static string GetRiskLevel(decimal riskScore)
    {
        if (riskScore >= 75)
        {
            return "Critical";
        }

        if (riskScore >= 60)
        {
            return "High";
        }

        if (riskScore >= 35)
        {
            return "Medium";
        }

        return "Low";
    }

    private static string BuildRiskSummary(Sprint? sprint, decimal blockedRatio, int openBugs, decimal releaseSuccessRate, decimal riskScore)
    {
        var sprintName = sprint?.SprintName ?? "latest delivery scope";
        return $"{sprintName} risk is {GetRiskLevel(riskScore).ToLowerInvariant()} at {riskScore:0.##}. Blocked work is {blockedRatio:0.##}% with {openBugs} open bugs and release success at {releaseSuccessRate:0.##}% .";
    }
}
