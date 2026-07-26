using System.Security.Claims;

namespace Sarathi.API.Application.DTOs;

public class MicrosoftUserInfo
{
    public string ObjectId { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string TenantId { get; init; } = string.Empty;

    public ClaimsPrincipal Principal { get; init; } = default!;
}