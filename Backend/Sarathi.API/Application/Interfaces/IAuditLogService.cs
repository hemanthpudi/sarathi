namespace Sarathi.API.Application.Interfaces;

public interface IAuditLogService
{
    Task LogLoginAsync(Guid userId, string ipAddress);
    Task LogLogoutAsync(Guid userId, string ipAddress);
}
