using System.ComponentModel.DataAnnotations;

namespace Sarathi.API.Application.DTOs.Notifications;

public class CreateNotificationRequestDto
{
    public Guid? UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = "General";

    [MaxLength(30)]
    public string Severity { get; set; } = "Info";

    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    public bool IsToast { get; set; } = true;
}