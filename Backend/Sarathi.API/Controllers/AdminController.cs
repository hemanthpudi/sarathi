using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sarathi.API.Application.DTOs.Admin;
using Sarathi.API.Application.DTOs.ProjectManager;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet("configure")]
    [ProducesResponseType(typeof(AdminConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConfigure(CancellationToken cancellationToken)
    {
        var response = await _adminService.GetConfigurationAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("configure")]
    [ProducesResponseType(typeof(AdminConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateConfigure(
        [FromBody] UpdateAdminConfigurationRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var updatedBy = GetAuthenticatedUserId();
        if (updatedBy == Guid.Empty)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _adminService.UpdateConfigurationAsync(request, updatedBy, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Invalid configuration payload received.");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("project-statistics")]
    [ProducesResponseType(typeof(AdminProjectStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProjectStatistics([FromQuery] int take = 20, CancellationToken cancellationToken = default)
    {
        var response = await _adminService.GetProjectStatisticsAsync(take, cancellationToken);
        return Ok(response);
    }

    [HttpGet("projects/{projectId}")]
    [ProducesResponseType(typeof(AdminProjectDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProjectDetails(int projectId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _adminService.GetProjectDetailsAsync(projectId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Project with ID {projectId} not found." });
        }
    }

    [HttpGet("projects/{projectId:int}/sprint-governance")]
    [ProducesResponseType(typeof(ProjectSprintGovernanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSprintGovernance(int projectId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _adminService.GetSprintGovernanceAsync(projectId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Project with ID {projectId} not found." });
        }
    }

    private Guid GetAuthenticatedUserId()
    {
        var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(subClaim, out var userId) ? userId : Guid.Empty;
    }
}
