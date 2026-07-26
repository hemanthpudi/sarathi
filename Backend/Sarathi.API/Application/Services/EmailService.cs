using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Sarathi.API.Application.DTOs.Email;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;

    public EmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendEmailAsync(SendEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(_options.FromAddress, _options.FromName);
        message.To.Add(request.To);
        message.Subject = request.Subject;
        message.Body = request.Body;
        message.IsBodyHtml = false;

        using var smtpClient = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_options.UserName, _options.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        await smtpClient.SendMailAsync(message, cancellationToken);
    }
}
