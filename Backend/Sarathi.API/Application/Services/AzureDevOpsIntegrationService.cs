using Microsoft.AspNetCore.DataProtection;
using Sarathi.API.Application.DTOs.AzureDevOps;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Domain.Entities;

namespace Sarathi.API.Application.Services;

public class AzureDevOpsIntegrationService : IAzureDevOpsIntegrationService
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IAzureDevOpsIntegrationRepository _azureDevOpsIntegrationRepository;
    private readonly IAzureDevOpsClient _azureDevOpsClient;
    private readonly IDataProtector _protector;
    private readonly string? _configuredPersonalAccessToken;

    public AzureDevOpsIntegrationService(
        IAdminConfigurationRepository adminConfigurationRepository,
        IAzureDevOpsIntegrationRepository azureDevOpsIntegrationRepository,
        IAzureDevOpsClient azureDevOpsClient,
        IDataProtectionProvider dataProtectionProvider,
        IConfiguration configuration)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _azureDevOpsIntegrationRepository = azureDevOpsIntegrationRepository;
        _azureDevOpsClient = azureDevOpsClient;
        _protector = dataProtectionProvider.CreateProtector("Sarathi.API.AzureDevOps.PAT");
        _configuredPersonalAccessToken = configuration["AzureDevOps:PersonalAccessToken"];
    }

    public async Task<AzureDevOpsIntegrationConfigurationDto> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await _adminConfigurationRepository.GetActiveAsync(cancellationToken)
            ?? CreateDefaultConfiguration();

        return ToConfigurationDto(configuration);
    }

    public async Task<AzureDevOpsIntegrationConfigurationDto> UpdateConfigurationAsync(UpdateAzureDevOpsIntegrationConfigurationRequestDto request, Guid? updatedByUserId, CancellationToken cancellationToken = default)
    {
        var configuration = await _adminConfigurationRepository.GetActiveAsync(cancellationToken)
            ?? CreateDefaultConfiguration();

        configuration.AzureDevOpsOrganizationUrl = request.OrganizationUrl.Trim();
        configuration.AzureDevOpsProjectFilter = request.ProjectFilter.Trim();
        configuration.AzureDevOpsSyncEnabled = request.SyncEnabled;
        configuration.UpdatedAtUtc = DateTime.UtcNow;
        configuration.UpdatedByUserId = updatedByUserId;
        configuration.IsActive = true;

        if (!string.IsNullOrWhiteSpace(request.PersonalAccessToken))
        {
            configuration.AzureDevOpsPatCipherText = _protector.Protect(request.PersonalAccessToken.Trim());
            configuration.AzureDevOpsPatUpdatedAtUtc = DateTime.UtcNow;
        }

        var updated = await _adminConfigurationRepository.UpsertAsync(configuration, cancellationToken);

        await _azureDevOpsIntegrationRepository.LogAsync(
            "Information",
            "AzureDevOpsConfiguration",
            "Azure DevOps organization configuration updated.",
            "Azure DevOps Integration",
            null,
            cancellationToken);

        return ToConfigurationDto(updated);
    }

    public async Task<AzureDevOpsLiveSummaryDto> GetLiveSummaryAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await _adminConfigurationRepository.GetActiveAsync(cancellationToken)
            ?? CreateDefaultConfiguration();

        var organizationUrl = configuration.AzureDevOpsOrganizationUrl.Trim();
        if (string.IsNullOrWhiteSpace(organizationUrl))
        {
            return CreateUnavailableLiveSummary("Azure DevOps organization URL is not configured.");
        }

        var personalAccessToken = ResolvePersonalAccessToken(configuration);
        if (string.IsNullOrWhiteSpace(personalAccessToken))
        {
            return CreateUnavailableLiveSummary("Azure DevOps PAT is not configured.", organizationUrl);
        }

        IReadOnlyList<AzureDevOpsProjectData> projects;
        try
        {
            projects = await _azureDevOpsClient.GetProjectsAsync(organizationUrl, personalAccessToken, cancellationToken);
        }
        catch (Exception ex)
        {
            return CreateUnavailableLiveSummary($"Failed to fetch Azure DevOps projects: {ex.Message}", organizationUrl);
        }

        var projectFilter = ParseProjectFilter(configuration.AzureDevOpsProjectFilter);
        var selectedProjects = projects
            .Where(item => projectFilter.Count == 0 || projectFilter.Contains(item.Name, StringComparer.OrdinalIgnoreCase))
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        var liveProjects = new List<AzureDevOpsLiveProjectDto>();
        foreach (var project in selectedProjects)
        {
            liveProjects.Add(await GetProjectSnapshotAsync(organizationUrl, personalAccessToken, project, cancellationToken));
        }

        var totalProjects = liveProjects.Count;
        var successfulProjects = liveProjects.Count(item => item.Status.Equals("Success", StringComparison.OrdinalIgnoreCase));
        var warningProjects = liveProjects.Count(item => item.Status.Equals("Warning", StringComparison.OrdinalIgnoreCase));
        var failedProjects = liveProjects.Count(item => item.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase));
        var totalItems = liveProjects.Sum(item => item.Items);
        var averageSyncTimeSeconds = liveProjects.Where(item => item.DurationSeconds.HasValue).Select(item => item.DurationSeconds!.Value).DefaultIfEmpty().Average();

        var timeline = liveProjects
            .Where(item => item.LatestActivityUtc.HasValue)
            .OrderByDescending(item => item.LatestActivityUtc)
            .Take(5)
            .Select(item => new AzureDevOpsLiveTimelineItemDto
            {
                ProjectName = item.ProjectName,
                Status = item.Status,
                Items = item.Items,
                TimeUtc = item.LatestActivityUtc,
                Message = item.LatestActivityMessage,
            })
            .ToList();

        var metrics = liveProjects
            .OrderBy(item => item.ProjectName, StringComparer.OrdinalIgnoreCase)
            .Select(item => new AzureDevOpsLiveMetricDto
            {
                ProjectName = item.ProjectName,
                Items = item.Items,
            })
            .ToList();

        return new AzureDevOpsLiveSummaryDto
        {
            IsConfigured = true,
            OrganizationUrl = organizationUrl,
            Message = "Live Azure DevOps data fetched successfully.",
            GeneratedAtUtc = DateTime.UtcNow,
            TotalProjects = totalProjects,
            SuccessfulProjects = successfulProjects,
            WarningProjects = warningProjects,
            FailedProjects = failedProjects,
            TotalItems = totalItems,
            AverageSyncTimeSeconds = averageSyncTimeSeconds,
            Projects = liveProjects,
            Timeline = timeline,
            Metrics = metrics,
        };
    }

    public async Task<AzureDevOpsConnectionTestResultDto> TestConnectionAsync(TestAzureDevOpsConnectionRequestDto request, CancellationToken cancellationToken = default)
    {
        var organizationUrl = request.OrganizationUrl.Trim();
        if (string.IsNullOrWhiteSpace(organizationUrl))
        {
            return new AzureDevOpsConnectionTestResultDto
            {
                IsConnected = false,
                Message = "Azure DevOps organization URL is required.",
                TestedAtUtc = DateTime.UtcNow,
            };
        }

        var personalAccessToken = request.PersonalAccessToken?.Trim();
        if (string.IsNullOrWhiteSpace(personalAccessToken))
        {
            return new AzureDevOpsConnectionTestResultDto
            {
                IsConnected = false,
                Message = "Azure DevOps PAT is required.",
                TestedAtUtc = DateTime.UtcNow,
            };
        }

        try
        {
            var projects = await _azureDevOpsClient.GetProjectsAsync(organizationUrl, personalAccessToken, cancellationToken);
            var filteredProjects = ParseProjectFilter(request.ProjectFilter).Count == 0
                ? projects
                : projects.Where(item => ParseProjectFilter(request.ProjectFilter).Contains(item.Name, StringComparer.OrdinalIgnoreCase)).ToList();

            return new AzureDevOpsConnectionTestResultDto
            {
                IsConnected = true,
                Message = $"Connection successful. {filteredProjects.Count} project(s) matched the filter.",
                TestedAtUtc = DateTime.UtcNow,
                ProjectCount = filteredProjects.Count,
            };
        }
        catch (Exception ex)
        {
            return new AzureDevOpsConnectionTestResultDto
            {
                IsConnected = false,
                Message = ex.Message,
                TestedAtUtc = DateTime.UtcNow,
            };
        }
    }

    public Task<IReadOnlyList<AzureDevOpsScheduleDto>> GetSchedulesAsync(CancellationToken cancellationToken = default)
    {
        return _azureDevOpsIntegrationRepository.GetSchedulesAsync(cancellationToken);
    }

    public Task<AzureDevOpsScheduleDto> UpsertScheduleAsync(UpsertAzureDevOpsScheduleRequestDto request, CancellationToken cancellationToken = default)
    {
        return _azureDevOpsIntegrationRepository.UpsertScheduleAsync(request, cancellationToken);
    }

    public Task<AzureDevOpsSyncQueueResponseDto> QueueSynchronizationAsync(QueueAzureDevOpsSyncRequestDto request, Guid? triggeredByUserId, string triggeredByDisplayName, CancellationToken cancellationToken = default)
    {
        request.SyncType = string.IsNullOrWhiteSpace(request.SyncType) ? "Full" : request.SyncType.Trim();
        request.ScopeName = string.IsNullOrWhiteSpace(request.ScopeName) ? "All Projects" : request.ScopeName.Trim();
        request.Source = string.IsNullOrWhiteSpace(request.Source) ? "Manual" : request.Source.Trim();

        return _azureDevOpsIntegrationRepository.QueueSynchronizationAsync(request, triggeredByUserId, triggeredByDisplayName, cancellationToken);
    }

    public Task<AzureDevOpsSyncJobsDto> GetSyncJobsAsync(int take, CancellationToken cancellationToken = default)
    {
        var normalizedTake = take <= 0 ? 50 : Math.Min(take, 200);
        return _azureDevOpsIntegrationRepository.GetSyncJobsAsync(normalizedTake, cancellationToken);
    }

    public Task<AzureDevOpsLogsDto> GetLogsAsync(int take, CancellationToken cancellationToken = default)
    {
        var normalizedTake = take <= 0 ? 100 : Math.Min(take, 500);
        return _azureDevOpsIntegrationRepository.GetLogsAsync(normalizedTake, cancellationToken);
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

    private AzureDevOpsIntegrationConfigurationDto ToConfigurationDto(AdminConfiguration configuration)
    {
        return new AzureDevOpsIntegrationConfigurationDto
        {
            OrganizationUrl = configuration.AzureDevOpsOrganizationUrl,
            ProjectFilter = configuration.AzureDevOpsProjectFilter,
            SyncEnabled = configuration.AzureDevOpsSyncEnabled,
            HasPersonalAccessTokenConfigured = !string.IsNullOrWhiteSpace(configuration.AzureDevOpsPatCipherText)
                || !string.IsNullOrWhiteSpace(_configuredPersonalAccessToken),
            PersonalAccessTokenUpdatedAtUtc = configuration.AzureDevOpsPatUpdatedAtUtc,
            UpdatedAtUtc = configuration.UpdatedAtUtc,
        };
    }

    private async Task<AzureDevOpsLiveProjectDto> GetProjectSnapshotAsync(string organizationUrl, string personalAccessToken, AzureDevOpsProjectData project, CancellationToken cancellationToken)
    {
        try
        {
            var workItemsTask = _azureDevOpsClient.GetWorkItemsAsync(organizationUrl, project.Name, personalAccessToken, cancellationToken);
            var repositoriesTask = _azureDevOpsClient.GetRepositoriesAsync(organizationUrl, project.Name, personalAccessToken, cancellationToken);
            var buildsTask = _azureDevOpsClient.GetBuildsAsync(organizationUrl, project.Name, personalAccessToken, cancellationToken);

            await Task.WhenAll(workItemsTask, repositoriesTask, buildsTask);

            var workItems = await workItemsTask;
            var repositories = await repositoriesTask;
            var builds = await buildsTask;

            var successfulBuilds = builds.Count(item => IsSuccessfulBuild(item.Result));
            var completedBuilds = builds.Count(item => !string.IsNullOrWhiteSpace(item.Result));
            var failedBuilds = builds.Count(item => IsFailedBuild(item.Result));
            var blockedItems = workItems.Count(item => item.IsBlocked);
            var buildSuccessRate = completedBuilds == 0 ? 100d : successfulBuilds * 100d / completedBuilds;
            var status = failedBuilds > 0 || buildSuccessRate < 50
                ? "Failed"
                : blockedItems > 0 || buildSuccessRate < 85
                    ? "Warning"
                    : "Success";

            DateTime? latestActivityUtc = builds
                .Select(item => item.FinishTimeUtc ?? item.StartTimeUtc)
                .Where(item => item.HasValue)
                .Max();

            latestActivityUtc ??= workItems.Select(item => item.LastUpdatedUtc).DefaultIfEmpty().Max();

            var durationSeconds = builds
                .Select(item => GetBuildDurationSeconds(item))
                .Where(item => item.HasValue)
                .Select(item => item!.Value)
                .DefaultIfEmpty()
                .Average();

            return new AzureDevOpsLiveProjectDto
            {
                ProjectName = project.Name,
                OrganizationName = GetOrganizationName(organizationUrl),
                Status = status,
                LastSyncUtc = latestActivityUtc,
                Items = workItems.Count + repositories.Count + builds.Count,
                DurationSeconds = durationSeconds > 0 ? durationSeconds : null,
                Errors = failedBuilds + blockedItems,
                LatestActivityUtc = latestActivityUtc,
                LatestActivityMessage = $"{workItems.Count} work items, {repositories.Count} repos, {builds.Count} builds",
            };
        }
        catch (Exception ex)
        {
            return new AzureDevOpsLiveProjectDto
            {
                ProjectName = project.Name,
                OrganizationName = GetOrganizationName(organizationUrl),
                Status = "Failed",
                Items = 0,
                Errors = 1,
                LatestActivityMessage = ex.Message,
            };
        }
    }

    private string ResolvePersonalAccessToken(AdminConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.AzureDevOpsPatCipherText))
        {
            return _protector.Unprotect(configuration.AzureDevOpsPatCipherText);
        }

        return _configuredPersonalAccessToken ?? string.Empty;
    }

    private static HashSet<string> ParseProjectFilter(string? projectFilter)
    {
        if (string.IsNullOrWhiteSpace(projectFilter))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return projectFilter
            .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static AzureDevOpsLiveSummaryDto CreateUnavailableLiveSummary(string message, string organizationUrl = "")
    {
        return new AzureDevOpsLiveSummaryDto
        {
            IsConfigured = false,
            OrganizationUrl = organizationUrl,
            Message = message,
            GeneratedAtUtc = DateTime.UtcNow,
        };
    }

    private static bool IsFailedBuild(string? result)
    {
        return string.Equals(result?.Trim(), "failed", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result?.Trim(), "canceled", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result?.Trim(), "partiallySucceeded", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSuccessfulBuild(string? result)
    {
        return string.Equals(result?.Trim(), "succeeded", StringComparison.OrdinalIgnoreCase);
    }

    private static double? GetBuildDurationSeconds(AzureDevOpsBuildData build)
    {
        if (build.StartTimeUtc is null || build.FinishTimeUtc is null)
        {
            return null;
        }

        return Math.Max(0, (build.FinishTimeUtc.Value - build.StartTimeUtc.Value).TotalSeconds);
    }

    private static string GetOrganizationName(string organizationUrl)
    {
        var trimmed = organizationUrl.Trim().TrimEnd('/');
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var parsed))
        {
            var host = parsed.Host;
            return host.StartsWith("dev.azure.com", StringComparison.OrdinalIgnoreCase)
                ? parsed.Segments.LastOrDefault()?.Trim('/') ?? host
                : host;
        }

        return trimmed;
    }
}
