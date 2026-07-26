using Sarathi.API.Application.DTOs.ProjectManager;

namespace Sarathi.API.Application.Interfaces;

public interface IProjectContextBuilder
{
    /// <summary>
    /// Builds a comprehensive project context from synchronized database data.
    /// This context includes all project information needed for AI analysis.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete project context</returns>
    Task<ProjectContextDto> BuildProjectContextAsync(int projectId, CancellationToken cancellationToken = default);
}
