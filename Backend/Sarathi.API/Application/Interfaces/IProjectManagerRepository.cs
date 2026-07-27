using Sarathi.API.Application.DTOs.ProjectManager;

namespace Sarathi.API.Application.Interfaces;

public interface IProjectManagerRepository
{
    Task<bool> IsProjectAssignedAsync(Guid userId, int projectId, CancellationToken cancellationToken = default);

    Task<bool> HasSynchronizedProjectDataAsync(CancellationToken cancellationToken = default);

    Task<ProjectManagerDashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectManagerAssignedProjectDto>> GetAssignedProjectsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ProjectManagerSprintProgressDto> GetSprintProgressAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ProjectManagerWorkItemsDto> GetWorkItemsAsync(Guid userId, int? projectId, string? state, int take, CancellationToken cancellationToken = default);

    Task<ProjectManagerKpisDto> GetKpisAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ProjectSprintGovernanceDto> GetSprintGovernanceAsync(Guid? userId, int projectId, CancellationToken cancellationToken = default);
}
