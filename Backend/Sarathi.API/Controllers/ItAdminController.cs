using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sarathi.API.Application.DTOs.ItAdmin;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Controllers;

[ApiController]
[Route("api/it-admin")]
[Authorize(Roles = "ITAdmin")]
[Produces("application/json")]
public class ItAdminController : ControllerBase
{
    private readonly IItAdminService _itAdminService;

    public ItAdminController(IItAdminService itAdminService)
    {
        _itAdminService = itAdminService;
    }

    [HttpGet("synchronization")]
    [ProducesResponseType(typeof(ItAdminSynchronizationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSynchronization(CancellationToken cancellationToken)
    {
        var response = await _itAdminService.GetSynchronizationAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("synchronization/run")]
    [ProducesResponseType(typeof(TriggerItAdminSyncResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RunSynchronization([FromBody] TriggerItAdminSyncRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _itAdminService.TriggerSynchronizationAsync(
            request,
            GetAuthenticatedUserId(),
            GetAuthenticatedDisplayName(),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("sync-monitor")]
    [ProducesResponseType(typeof(ItAdminSyncMonitoringDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSyncMonitor(CancellationToken cancellationToken)
    {
        var response = await _itAdminService.GetSyncMonitoringAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("logs")]
    [ProducesResponseType(typeof(ItAdminLogsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs([FromQuery] string? severity, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var response = await _itAdminService.GetLogsAsync(severity, take, cancellationToken);
        return Ok(response);
    }

    [HttpGet("maintenance")]
    [ProducesResponseType(typeof(ItAdminMaintenanceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaintenance(CancellationToken cancellationToken)
    {
        var response = await _itAdminService.GetMaintenanceAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("scheduling")]
    [ProducesResponseType(typeof(ItAdminSchedulingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScheduling(CancellationToken cancellationToken)
    {
        var response = await _itAdminService.GetSchedulingAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("system-health")]
    [ProducesResponseType(typeof(ItAdminSystemHealthDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemHealth(CancellationToken cancellationToken)
    {
        return Ok(await _itAdminService.GetSystemHealthAsync(cancellationToken));
    }

    [HttpGet("login-statistics")]
    [ProducesResponseType(typeof(ItAdminLoginStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoginStatistics(CancellationToken cancellationToken)
    {
        return Ok(await _itAdminService.GetLoginStatisticsAsync(cancellationToken));
    }

    [HttpGet("login-audits")]
    [ProducesResponseType(typeof(IReadOnlyList<ItAdminLoginAuditDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoginAudits([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        return Ok(await _itAdminService.GetLoginAuditsAsync(take, cancellationToken));
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<ItAdminUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        return Ok(await _itAdminService.GetUsersAsync(cancellationToken));
    }

    [HttpPut("users/{userId:guid}/role")]
    [ProducesResponseType(typeof(ItAdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateItAdminUserRoleRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var user = await _itAdminService.UpdateUserRoleAsync(userId, request.Role, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    private Guid? GetAuthenticatedUserId()
    {
        var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(subClaim, out var userId) ? userId : null;
    }

    private string GetAuthenticatedDisplayName()
    {
        return User.FindFirst("name")?.Value
               ?? User.FindFirst(ClaimTypes.Name)?.Value
               ?? "IT Admin";
    }
}
