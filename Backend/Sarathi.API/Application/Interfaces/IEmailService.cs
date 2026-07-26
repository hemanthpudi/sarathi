using Sarathi.API.Application.DTOs.Email;

namespace Sarathi.API.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(SendEmailRequestDto request, CancellationToken cancellationToken = default);
}
