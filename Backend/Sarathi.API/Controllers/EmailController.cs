using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Sarathi.API.Application.DTOs.Email;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Controllers;

[ApiController]
[Route("api/email")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("To, Subject, and Body are required.");
        }

        try
        {
            await _emailService.SendEmailAsync(request, cancellationToken);
            return NoContent();
        }
        catch (SmtpException smtpException)
        {
            return Problem(
                detail: "SMTP authentication failed or secure connection could not be established. Verify the configured SMTP host, port, credentials, and SSL settings.",
                statusCode: StatusCodes.Status502BadGateway,
                title: "Email delivery failed");
        }
    }
}
