using Sarathi.API.Application.DTOs.ItAdmin;

namespace Sarathi.API.Application.Interfaces;

public interface IItAdminRepository
{
    Task<ItAdminSynchronizationDto> GetSynchronizationAsync(CancellationToken cancellationToken = default);

    Task<ItAdminSyncMonitoringDto> GetSyncMonitoringAsync(CancellationToken cancellationToken = default);

    Task<ItAdminLogsDto> GetLogsAsync(string? severity, int take, CancellationToken cancellationToken = default);

    Task<ItAdminMaintenanceDto> GetMaintenanceAsync(CancellationToken cancellationToken = default);

    Task<ItAdminSchedulingDto> GetSchedulingAsync(CancellationToken cancellationToken = default);

    Task<TriggerItAdminSyncResponseDto> TriggerSynchronizationAsync(TriggerItAdminSyncRequestDto request, Guid? triggeredByUserId, string triggeredByDisplayName, CancellationToken cancellationToken = default);

    Task<ItAdminLoginStatisticsDto> GetLoginStatisticsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ItAdminLoginAuditDto>> GetLoginAuditsAsync(int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ItAdminUserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<ItAdminUserDto?> UpdateUserRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
    Task<ItAdminSystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default);
}
