using Sarathi.API.Application.DTOs;

namespace Sarathi.API.Application.Interfaces;

public interface IMicrosoftTokenValidator
{
    Task<MicrosoftUserInfo> ValidateAsync(string microsoftToken);
}