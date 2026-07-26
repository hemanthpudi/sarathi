using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Domain.Entities;
using Sarathi.API.Persistence;

namespace Sarathi.API.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(AppDbContext dbContext, ILogger<AuditLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogLoginAsync(Guid userId, string ipAddress)
    {
        var audit = new LoginAudit
        {
            AuditId = Guid.NewGuid(),
            UserId = userId,
            LoginTime = DateTime.UtcNow,
            LogoutTime = null,
            IPAddress = ipAddress,
            Status = "Success"
        };

        _dbContext.LoginAudits.Add(audit);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Login audit recorded for user {UserId} from IP {IPAddress}.", userId, ipAddress);
    }

    public async Task LogLogoutAsync(Guid userId, string ipAddress)
    {
        var latestAudit = await _dbContext.LoginAudits
            .Where(a => a.UserId == userId && a.LogoutTime == null)
            .OrderByDescending(a => a.LoginTime)
            .FirstOrDefaultAsync();

        if (latestAudit is not null)
        {
            latestAudit.LogoutTime = DateTime.UtcNow;
            latestAudit.Status = "LoggedOut";
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            var audit = new LoginAudit
            {
                AuditId = Guid.NewGuid(),
                UserId = userId,
                LoginTime = DateTime.UtcNow,
                LogoutTime = DateTime.UtcNow,
                IPAddress = ipAddress,
                Status = "LoggedOut"
            };
            _dbContext.LoginAudits.Add(audit);
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("Logout audit recorded for user {UserId} from IP {IPAddress}.", userId, ipAddress);
    }
}
