using System.ComponentModel.DataAnnotations;

namespace Sarathi.API.Application.DTOs.ItAdmin;

public class ItAdminLoginAuditDto
{
    public Guid AuditId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ItAdminLoginStatisticsDto
{
    public int TotalLogins24Hours { get; set; }
    public int ActiveSessions { get; set; }
    public int UniqueUsers24Hours { get; set; }
    public int LoggedOut24Hours { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
}

public class ItAdminUserDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? LastLoginUtc { get; set; }
}

public class UpdateItAdminUserRoleRequestDto
{
    [Required]
    [RegularExpression("^(Administrator|ProjectManager|ITAdmin)$")]
    public string Role { get; set; } = string.Empty;
}

public class ItAdminSystemHealthDto
{
    public string Status { get; set; } = "Healthy";
    public bool DatabaseReachable { get; set; }
    public bool AzureDevOpsConfigured { get; set; }
    public bool SynchronizationEnabled { get; set; }
    public int RunningSynchronizationJobs { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
}
