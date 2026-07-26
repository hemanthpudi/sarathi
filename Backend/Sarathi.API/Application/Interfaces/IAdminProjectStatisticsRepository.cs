using Sarathi.API.Application.DTOs.Admin;

namespace Sarathi.API.Application.Interfaces;

public interface IAdminProjectStatisticsRepository
{
    Task<AdminProjectStatisticsDto> GetProjectStatisticsAsync(int take, CancellationToken cancellationToken = default);
}
