namespace Sarathi.API.Domain.Entities;

public class AzureDevOpsSyncJob
{
    public int Id { get; set; }

    public string SyncType { get; set; } = string.Empty;

    public string ScopeName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string TriggeredByDisplayName { get; set; } = string.Empty;

    public Guid? TriggeredByUserId { get; set; }

    public int ItemsProcessed { get; set; }

    public int ItemsSucceeded { get; set; }

    public int ItemsFailed { get; set; }

    public int? DurationSeconds { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }
}