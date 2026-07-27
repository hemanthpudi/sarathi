using Sarathi.API.Application.DTOs.ProjectManager;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Application.Services;

public class ProjectManagerService : IProjectManagerService
{
    private readonly IProjectManagerRepository _projectManagerRepository;
    private readonly IAzureDevOpsSynchronizationRuntimeService _azureDevOpsSyncRuntimeService;

    public ProjectManagerService(
        IProjectManagerRepository projectManagerRepository,
        IAzureDevOpsSynchronizationRuntimeService azureDevOpsSyncRuntimeService)
    {
        _projectManagerRepository = projectManagerRepository;
        _azureDevOpsSyncRuntimeService = azureDevOpsSyncRuntimeService;
    }

    public Task<bool> IsProjectAssignedAsync(Guid userId, int projectId, CancellationToken cancellationToken = default)
    {
        return _projectManagerRepository.IsProjectAssignedAsync(userId, projectId, cancellationToken);
    }

    public async Task<ProjectManagerDashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        return await _projectManagerRepository.GetDashboardAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectManagerAssignedProjectDto>> GetAssignedProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        return await _projectManagerRepository.GetAssignedProjectsAsync(userId, cancellationToken);
    }

    public async Task<ProjectManagerSprintProgressDto> GetSprintProgressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        return await _projectManagerRepository.GetSprintProgressAsync(userId, cancellationToken);
    }

    public async Task<ProjectManagerWorkItemsDto> GetWorkItemsAsync(Guid userId, int? projectId, string? state, int take, CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        var normalizedTake = take <= 0 ? 50 : Math.Min(take, 250);
        var normalizedState = string.IsNullOrWhiteSpace(state) ? null : state.Trim();

        return await _projectManagerRepository.GetWorkItemsAsync(userId, projectId, normalizedState, normalizedTake, cancellationToken);
    }

    public async Task<ProjectManagerKpisDto> GetKpisAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        return await _projectManagerRepository.GetKpisAsync(userId, cancellationToken);
    }

    public async Task<ProjectSprintGovernanceDto> GetSprintGovernanceAsync(Guid? userId, int projectId, CancellationToken cancellationToken = default)
    {
        await EnsureLiveDataAsync(cancellationToken);
        return await _projectManagerRepository.GetSprintGovernanceAsync(userId, projectId, cancellationToken);
    }

    private async Task EnsureLiveDataAsync(CancellationToken cancellationToken)
    {
        if (!await _projectManagerRepository.HasSynchronizedProjectDataAsync(cancellationToken))
        {
            await _azureDevOpsSyncRuntimeService.SynchronizeNowAsync(cancellationToken);
        }
    }
}
