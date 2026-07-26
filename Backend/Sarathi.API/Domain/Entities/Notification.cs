namespace Sarathi.API.Domain.Entities;

public class Notification
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; }

    public bool IsToast { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAtUtc { get; set; }

    public User? User { get; set; }
}