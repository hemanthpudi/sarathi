using Sarathi.API.Application.DTOs.Admin;
using Sarathi.API.Application.DTOs.ProjectManager;

namespace Sarathi.API.Application.Interfaces;

public interface IAdminService
{
    Task<AdminConfigurationDto> GetConfigurationAsync(CancellationToken cancellationToken = default);

    Task<AdminConfigurationDto> UpdateConfigurationAsync(UpdateAdminConfigurationRequestDto request, Guid updatedByUserId, CancellationToken cancellationToken = default);

    Task<AdminProjectStatisticsDto> GetProjectStatisticsAsync(int take, CancellationToken cancellationToken = default);

    Task<AdminProjectDetailsDto> GetProjectDetailsAsync(int projectId, CancellationToken cancellationToken = default);

    Task<ProjectSprintGovernanceDto> GetSprintGovernanceAsync(int projectId, CancellationToken cancellationToken = default);
}
