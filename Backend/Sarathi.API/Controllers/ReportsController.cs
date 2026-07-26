using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sarathi.API.Application.DTOs.Reports;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    [HttpGet("overview")]
    [ProducesResponseType(typeof(ReportsOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOverview([FromQuery] int? projectId, CancellationToken cancellationToken)
    {
        var response = await _reportsService.GetOverviewAsync(GetAuthenticatedUserId(), GetAuthenticatedRole(), projectId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("export/{sectionKey}")]
    [ProducesResponseType(typeof(ExportReportResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportSection(string sectionKey, [FromQuery] int? projectId, CancellationToken cancellationToken)
    {
        var response = await _reportsService.ExportSectionAsync(sectionKey, GetAuthenticatedUserId(), GetAuthenticatedRole(), projectId, cancellationToken);
        return Ok(response);
    }

    private Guid? GetAuthenticatedUserId()
    {
        var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(subClaim, out var userId) ? userId : null;
    }

    private string? GetAuthenticatedRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
    }
}
