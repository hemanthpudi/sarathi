using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sarathi.API.Application.DTOs.AzureDevOps;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Controllers;

[ApiController]
[Route("api/integrations/azure-devops")]
[Authorize(Roles = "Administrator,ITAdmin")]
[Produces("application/json")]
public class AzureDevOpsIntegrationController : ControllerBase
{
    private readonly IAzureDevOpsIntegrationService _azureDevOpsIntegrationService;

    public AzureDevOpsIntegrationController(IAzureDevOpsIntegrationService azureDevOpsIntegrationService)
    {
        _azureDevOpsIntegrationService = azureDevOpsIntegrationService;
    }

    [HttpGet("configuration")]
    [Authorize(Roles = "Administrator,ITAdmin")]
    [ProducesResponseType(typeof(AzureDevOpsIntegrationConfigurationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfiguration(CancellationToken cancellationToken)
    {
        var response = await _azureDevOpsIntegrationService.GetConfigurationAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("configuration")]
    [Authorize(Roles = "Administrator,ITAdmin")]
    [ProducesResponseType(typeof(AzureDevOpsIntegrationConfigurationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateConfiguration([FromBody] UpdateAzureDevOpsIntegrationConfigurationRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await _azureDevOpsIntegrationService.UpdateConfigurationAsync(request, GetAuthenticatedUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("live-summary")]
    [ProducesResponseType(typeof(AzureDevOpsLiveSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiveSummary(CancellationToken cancellationToken)
    {
        var response = await _azureDevOpsIntegrationService.GetLiveSummaryAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("test-connection")]
    [ProducesResponseType(typeof(AzureDevOpsConnectionTestResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestConnection([FromBody] TestAzureDevOpsConnectionRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await _azureDevOpsIntegrationService.TestConnectionAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("schedules")]
    [ProducesResponseType(typeof(IReadOnlyList<AzureDevOpsScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchedules(CancellationToken cancellationToken)
    {
        var response = await _azureDevOpsIntegrationService.GetSchedulesAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("schedules")]
    [ProducesResponseType(typeof(AzureDevOpsScheduleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertSchedule([FromBody] UpsertAzureDevOpsScheduleRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await _azureDevOpsIntegrationService.UpsertScheduleAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("sync-jobs")]
    [ProducesResponseType(typeof(AzureDevOpsSyncQueueResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> QueueSyncJob([FromBody] QueueAzureDevOpsSyncRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await _azureDevOpsIntegrationService.QueueSynchronizationAsync(
            request,
            GetAuthenticatedUserId(),
            GetAuthenticatedDisplayName(),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("sync-jobs")]
    [ProducesResponseType(typeof(AzureDevOpsSyncJobsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSyncJobs([FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var response = await _azureDevOpsIntegrationService.GetSyncJobsAsync(take, cancellationToken);
        return Ok(response);
    }

    [HttpGet("logs")]
    [ProducesResponseType(typeof(AzureDevOpsLogsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var response = await _azureDevOpsIntegrationService.GetLogsAsync(take, cancellationToken);
        return Ok(response);
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
               ?? "Azure DevOps Integration";
    }
}
