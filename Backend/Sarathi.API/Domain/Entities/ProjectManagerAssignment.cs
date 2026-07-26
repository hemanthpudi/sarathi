namespace Sarathi.API.Domain.Entities;

public class ProjectManagerAssignment
{
    public int Id { get; set; }

    public Guid ProjectManagerUserId { get; set; }

    public int ProjectId { get; set; }

    public string ProjectRole { get; set; } = string.Empty;

    public decimal AllocationPercent { get; set; }

    public string DeliveryHealth { get; set; } = "On Track";

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? PlannedEndDateUtc { get; set; }

    public Project? Project { get; set; }
}