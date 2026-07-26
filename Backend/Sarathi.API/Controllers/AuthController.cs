using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sarathi.API.Application.DTOs;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user via Microsoft Entra ID access token and returns a JWT.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.MicrosoftToken))
            return BadRequest(ProblemDetailsFor(400, "Microsoft token is required.", HttpContext));

        var ipAddress = GetClientIpAddress();

        try
        {
            var response = await _authService.LoginAsync(request.MicrosoftToken, ipAddress);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized login attempt from IP {IP}.", ipAddress);
            return Unauthorized(ProblemDetailsFor(401, ex.Message, HttpContext));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Login failed — user not found.");
            return NotFound(ProblemDetailsFor(404, ex.Message, HttpContext));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login.");
            return StatusCode(500, ProblemDetailsFor(500, "An internal error occurred.", HttpContext));
        }
    }

    /// <summary>
    /// Returns the current authenticated user's profile.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Me()
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty)
            return Unauthorized(ProblemDetailsFor(401, "Invalid or missing token.", HttpContext));

        try
        {
            var userDto = await _authService.GetCurrentUserAsync(userId);
            return Ok(userDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "GetMe: User {UserId} not found.", userId);
            return NotFound(ProblemDetailsFor(404, ex.Message, HttpContext));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in /me.");
            return StatusCode(500, ProblemDetailsFor(500, "An internal error occurred.", HttpContext));
        }
    }

    /// <summary>
    /// Logs out the current user and records the logout audit.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty)
            return Unauthorized(ProblemDetailsFor(401, "Invalid or missing token.", HttpContext));

        var ipAddress = GetClientIpAddress();

        try
        {
            await _authService.LogoutAsync(userId, ipAddress);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Logout: User {UserId} not found.", userId);
            return NotFound(ProblemDetailsFor(404, ex.Message, HttpContext));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during logout.");
            return StatusCode(500, ProblemDetailsFor(500, "An internal error occurred.", HttpContext));
        }
    }

    /// <summary>
    /// Issues a new JWT for the current authenticated user.
    /// </summary>
    [Authorize]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh()
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty)
            return Unauthorized(ProblemDetailsFor(401, "Invalid or missing token.", HttpContext));

        try
        {
            var response = await _authService.RefreshTokenAsync(userId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Refresh denied for user {UserId}.", userId);
            return Unauthorized(ProblemDetailsFor(401, ex.Message, HttpContext));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Refresh: User {UserId} not found.", userId);
            return NotFound(ProblemDetailsFor(404, ex.Message, HttpContext));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh.");
            return StatusCode(500, ProblemDetailsFor(500, "An internal error occurred.", HttpContext));
        }
    }

    private Guid GetAuthenticatedUserId()
    {
        var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(subClaim, out var userId) ? userId : Guid.Empty;
    }

    private string GetClientIpAddress()
    {
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstIp = forwardedFor.Split(',')[0].Trim();
            if (IPAddress.TryParse(firstIp, out _))
                return firstIp;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static ProblemDetails ProblemDetailsFor(int status, string detail, HttpContext context)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = status switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                _ => "Internal Server Error"
            },
            Detail = detail,
            Instance = context.Request.Path
        };
    }
}
