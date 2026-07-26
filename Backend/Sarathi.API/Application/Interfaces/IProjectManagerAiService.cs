using Sarathi.API.Application.DTOs.ProjectManager;

namespace Sarathi.API.Application.Interfaces;

public interface IProjectManagerAiService
{
    Task<ProjectManagerAiAnalysisDto> AnalyzeProjectAsync(Guid userId, int projectId, string? question, CancellationToken cancellationToken = default);
    Task<ProjectManagerAiAnalysisDto> AnalyzeProjectAsync(int projectId, string? question, CancellationToken cancellationToken = default);
}
