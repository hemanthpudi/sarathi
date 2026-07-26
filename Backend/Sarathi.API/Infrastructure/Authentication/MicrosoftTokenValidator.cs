using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Application.DTOs;

namespace Sarathi.API.Infrastructure.Authentication;

public class MicrosoftTokenValidator : IMicrosoftTokenValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MicrosoftTokenValidator> _logger;
    private IConfigurationManager<OpenIdConnectConfiguration>? _configManager;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public MicrosoftTokenValidator(IConfiguration configuration, ILogger<MicrosoftTokenValidator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<IConfigurationManager<OpenIdConnectConfiguration>> GetConfigManagerAsync()
    {
        if (_configManager is not null)
            return _configManager;

        await _lock.WaitAsync();
        try
        {
            if (_configManager is not null)
                return _configManager;

            var tenantId = _configuration["AzureAd:TenantId"]
                ?? throw new InvalidOperationException("AzureAd:TenantId is not configured.");

            var metadataAddress =
                $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";

            _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true });

            return _configManager;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MicrosoftUserInfo> ValidateAsync(string microsoftToken)   
 {
        if (string.IsNullOrWhiteSpace(microsoftToken))
            throw new ArgumentException("Microsoft token must not be empty.", nameof(microsoftToken));

        var configManager = await GetConfigManagerAsync();
        var openIdConfig = await configManager.GetConfigurationAsync(CancellationToken.None);

        var clientId = _configuration["AzureAd:ClientId"]
            ?? throw new InvalidOperationException("AzureAd:ClientId is not configured.");
        var configuredAudience = _configuration["AzureAd:Audience"];
        var validAudiences = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            clientId,
            $"api://{clientId}"
        };

        if (!string.IsNullOrWhiteSpace(configuredAudience))
            validAudiences.Add(configuredAudience);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/{_configuration["AzureAd:TenantId"]}/v2.0",
                $"https://sts.windows.net/{_configuration["AzureAd:TenantId"]}/"
            },
            ValidateAudience = true,
            ValidAudiences = validAudiences,
            ValidateLifetime = true,
            IssuerSigningKeys = openIdConfig.SigningKeys,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
var principal = handler.ValidateToken(microsoftToken, validationParameters, out _);

var objectId =
    principal.FindFirst("oid")?.Value
    ?? string.Empty;

var email =
    principal.FindFirst("preferred_username")?.Value
    ?? principal.FindFirst("upn")?.Value
    ?? principal.FindFirst("email")?.Value
    ?? principal.FindFirst(ClaimTypes.Email)?.Value
    ?? string.Empty;

var name =
    principal.FindFirst("name")?.Value
    ?? principal.FindFirst(ClaimTypes.Name)?.Value
    ?? email;

var tenantId =
    principal.FindFirst("tid")?.Value
    ?? string.Empty;

return new MicrosoftUserInfo
{
    ObjectId = objectId,
    Email = email,
    Name = name,
    TenantId = tenantId,
    Principal = principal
};
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Microsoft token validation failed.");
            throw new UnauthorizedAccessException("Microsoft access token is invalid or expired.", ex);
        }
    }
}
