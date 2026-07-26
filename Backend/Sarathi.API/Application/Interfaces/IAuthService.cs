using Sarathi.API.Application.DTOs;

namespace Sarathi.API.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(string microsoftToken, string ipAddress);
    Task<UserDto> GetCurrentUserAsync(Guid userId);
    Task LogoutAsync(Guid userId, string ipAddress);
    Task<LoginResponseDto> RefreshTokenAsync(Guid userId);
}
