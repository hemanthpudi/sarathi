namespace Sarathi.API.Domain.Entities;

public class KpiSnapshot
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;

    public decimal? SprintVelocity { get; set; }

    public decimal? CompletionRate { get; set; }

    public decimal? DefectDensity { get; set; }

    public decimal? BacklogHealth { get; set; }

    public decimal? ReleaseSuccessRate { get; set; }

    public Project? Project { get; set; }
}
