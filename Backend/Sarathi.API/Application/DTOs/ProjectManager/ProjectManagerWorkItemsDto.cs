namespace Sarathi.API.Application.DTOs.ProjectManager;

public class ProjectManagerWorkItemsDto
{
    public int TotalWorkItems { get; set; }

    public int OpenWorkItems { get; set; }

    public int InProgressWorkItems { get; set; }

    public int BlockedWorkItems { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<ProjectManagerWorkItemDto> Items { get; set; } = new();
}