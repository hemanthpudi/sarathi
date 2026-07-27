using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sarathi.API.Application.DTOs.ProjectManager;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Controllers;

[ApiController]
[Route("api/project-manager")]
[Authorize(Roles = "ProjectManager")]
[Produces("application/json")]
public class ProjectManagerController : ControllerBase
{
    private readonly IProjectManagerService _projectManagerService;
    private readonly IProjectManagerAiService _projectManagerAiService;

    public ProjectManagerController(IProjectManagerService projectManagerService, IProjectManagerAiService projectManagerAiService)
    {
        _projectManagerService = projectManagerService;
        _projectManagerAiService = projectManagerAiService;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ProjectManagerDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var response = await _projectManagerService.GetDashboardAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("assigned-projects")]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectManagerAssignedProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAssignedProjects(CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var response = await _projectManagerService.GetAssignedProjectsAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("sprint-progress")]
    [ProducesResponseType(typeof(ProjectManagerSprintProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSprintProgress(CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var response = await _projectManagerService.GetSprintProgressAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("work-items")]
    [ProducesResponseType(typeof(ProjectManagerWorkItemsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWorkItems([FromQuery] int? projectId, [FromQuery] string? state, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (projectId.HasValue && !await _projectManagerService.IsProjectAssignedAsync(userId, projectId.Value, cancellationToken))
        {
            return Forbid();
        }

        var response = await _projectManagerService.GetWorkItemsAsync(userId, projectId, state, take, cancellationToken);
        return Ok(response);
    }

    [HttpGet("kpis")]
    [ProducesResponseType(typeof(ProjectManagerKpisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetKpis(CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var response = await _projectManagerService.GetKpisAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("projects/{projectId:int}/sprint-governance")]
    [ProducesResponseType(typeof(ProjectSprintGovernanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSprintGovernance(int projectId, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (!await _projectManagerService.IsProjectAssignedAsync(userId, projectId, cancellationToken))
        {
            return Forbid();
        }

        return Ok(await _projectManagerService.GetSprintGovernanceAsync(userId, projectId, cancellationToken));
    }

    [HttpPost("projects/{projectId:int}/ai-analysis")]
    [ProducesResponseType(typeof(ProjectManagerAiAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAiAnalysis(int projectId, [FromBody] ProjectManagerAiAnalysisRequestDto? request, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty) return Unauthorized();
        if (!await _projectManagerService.IsProjectAssignedAsync(userId, projectId, cancellationToken)) return Forbid();
        return Ok(await _projectManagerAiService.AnalyzeProjectAsync(userId, projectId, request?.Question, cancellationToken));
    }

    private Guid GetAuthenticatedUserId()
    {
        var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(subClaim, out var userId) ? userId : Guid.Empty;
    }
}
