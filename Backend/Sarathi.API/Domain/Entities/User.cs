using Sarathi.API.Domain.Enums;

namespace Sarathi.API.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string EntraObjectId { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public ICollection<LoginAudit> LoginAudits { get; set; }
        = new List<LoginAudit>();

    public ICollection<Notification> Notifications { get; set; }
        = new List<Notification>();
}