using Sarathi.API.Domain.Entities;

namespace Sarathi.API.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    Guid? ValidateTokenAndGetUserId(string token);
    DateTime GetTokenExpiry();
}
