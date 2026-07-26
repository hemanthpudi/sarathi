using Sarathi.API.Application.DTOs.ProjectManager;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Application.Services;

public class ProjectManagerService : IProjectManagerService
{
    private readonly IProjectManagerRepository _projectManagerRepository;

    public ProjectManagerService(IProjectManagerRepository projectManagerRepository)
    {
        _projectManagerRepository = projectManagerRepository;
    }

    public Task<bool> IsProjectAssignedAsync(Guid userId, int projectId, CancellationToken cancellationToken = default)
    {
        return _projectManagerRepository.IsProjectAssignedAsync(userId, projectId, cancellationToken);
    }

    public Task<ProjectManagerDashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _projectManagerRepository.GetDashboardAsync(userId, cancellationToken);
    }

    public Task<IReadOnlyList<ProjectManagerAssignedProjectDto>> GetAssignedProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _projectManagerRepository.GetAssignedProjectsAsync(userId, cancellationToken);
    }

    public Task<ProjectManagerSprintProgressDto> GetSprintProgressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _projectManagerRepository.GetSprintProgressAsync(userId, cancellationToken);
    }

    public Task<ProjectManagerWorkItemsDto> GetWorkItemsAsync(Guid userId, int? projectId, string? state, int take, CancellationToken cancellationToken = default)
    {
        var normalizedTake = take <= 0 ? 50 : Math.Min(take, 250);
        var normalizedState = string.IsNullOrWhiteSpace(state) ? null : state.Trim();

        return _projectManagerRepository.GetWorkItemsAsync(userId, projectId, normalizedState, normalizedTake, cancellationToken);
    }

    public Task<ProjectManagerKpisDto> GetKpisAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _projectManagerRepository.GetKpisAsync(userId, cancellationToken);
    }

    public Task<ProjectSprintGovernanceDto> GetSprintGovernanceAsync(Guid? userId, int projectId, CancellationToken cancellationToken = default)
    {
        return _projectManagerRepository.GetSprintGovernanceAsync(userId, projectId, cancellationToken);
    }
}
