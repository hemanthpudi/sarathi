namespace Sarathi.API.Domain.Entities;

public class AdminLogEntry
{
    public int Id { get; set; }

    public string Severity { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string SourceSystem { get; set; } = string.Empty;

    public string? CorrelationId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}