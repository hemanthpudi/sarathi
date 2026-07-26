namespace Sarathi.API.Application.DTOs.ProjectManager;

public class ProjectManagerSprintProgressDto
{
    public DateTime GeneratedAtUtc { get; set; }

    public List<ProjectManagerSprintItemDto> Sprints { get; set; } = new();
}