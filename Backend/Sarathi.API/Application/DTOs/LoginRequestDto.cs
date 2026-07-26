namespace Sarathi.API.Application.DTOs;

public class LoginRequestDto
{
    public string MicrosoftToken { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
}
