namespace Sarathi.API.Domain.Entities;

public class LoginAudit
{
    public Guid AuditId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}
