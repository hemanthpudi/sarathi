using Sarathi.API.Application.DTOs.ItAdmin;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Application.Services;

public class ItAdminService : IItAdminService
{
    private readonly IItAdminRepository _itAdminRepository;

    public ItAdminService(IItAdminRepository itAdminRepository)
    {
        _itAdminRepository = itAdminRepository;
    }

    public Task<ItAdminSynchronizationDto> GetSynchronizationAsync(CancellationToken cancellationToken = default)
    {
        return _itAdminRepository.GetSynchronizationAsync(cancellationToken);
    }

    public Task<ItAdminSyncMonitoringDto> GetSyncMonitoringAsync(CancellationToken cancellationToken = default)
    {
        return _itAdminRepository.GetSyncMonitoringAsync(cancellationToken);
    }

    public Task<ItAdminLogsDto> GetLogsAsync(string? severity, int take, CancellationToken cancellationToken = default)
    {
        var normalizedSeverity = string.IsNullOrWhiteSpace(severity) ? null : severity.Trim();
        var normalizedTake = take <= 0 ? 50 : Math.Min(take, 250);
        return _itAdminRepository.GetLogsAsync(normalizedSeverity, normalizedTake, cancellationToken);
    }

    public Task<ItAdminMaintenanceDto> GetMaintenanceAsync(CancellationToken cancellationToken = default)
    {
        return _itAdminRepository.GetMaintenanceAsync(cancellationToken);
    }

    public Task<ItAdminSchedulingDto> GetSchedulingAsync(CancellationToken cancellationToken = default)
    {
        return _itAdminRepository.GetSchedulingAsync(cancellationToken);
    }

    public Task<TriggerItAdminSyncResponseDto> TriggerSynchronizationAsync(TriggerItAdminSyncRequestDto request, Guid? triggeredByUserId, string triggeredByDisplayName, CancellationToken cancellationToken = default)
    {
        request.SyncType = string.IsNullOrWhiteSpace(request.SyncType) ? "Full" : request.SyncType.Trim();
        request.ScopeName = string.IsNullOrWhiteSpace(request.ScopeName) ? "All Projects" : request.ScopeName.Trim();
        request.Source = string.IsNullOrWhiteSpace(request.Source) ? "Manual" : request.Source.Trim();

        return _itAdminRepository.TriggerSynchronizationAsync(request, triggeredByUserId, triggeredByDisplayName, cancellationToken);
    }

    public Task<ItAdminLoginStatisticsDto> GetLoginStatisticsAsync(CancellationToken cancellationToken = default) => _itAdminRepository.GetLoginStatisticsAsync(cancellationToken);

    public Task<IReadOnlyList<ItAdminLoginAuditDto>> GetLoginAuditsAsync(int take, CancellationToken cancellationToken = default) => _itAdminRepository.GetLoginAuditsAsync(Math.Clamp(take, 1, 500), cancellationToken);

    public Task<IReadOnlyList<ItAdminUserDto>> GetUsersAsync(CancellationToken cancellationToken = default) => _itAdminRepository.GetUsersAsync(cancellationToken);

    public Task<ItAdminUserDto?> UpdateUserRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default) => _itAdminRepository.UpdateUserRoleAsync(userId, role, cancellationToken);

    public Task<ItAdminSystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default) => _itAdminRepository.GetSystemHealthAsync(cancellationToken);
}
