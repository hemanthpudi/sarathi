using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Sarathi.API.Application.DTOs;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Domain.Entities;
using Sarathi.API.Domain.Enums;
using Sarathi.API.Persistence;

namespace Sarathi.API.Application.Services;

public class AuthService : IAuthService
{
    private static readonly ConcurrentDictionary<Guid, User> DevUsers = new();

    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IMicrosoftTokenValidator _microsoftTokenValidator;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext dbContext,
        IConfiguration configuration,
        IMicrosoftTokenValidator microsoftTokenValidator,
        IJwtTokenService jwtTokenService,
        IAuditLogService auditLogService,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _microsoftTokenValidator = microsoftTokenValidator;
        _jwtTokenService = jwtTokenService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(string microsoftToken, string ipAddress)
    {
        var microsoftUser = await _microsoftTokenValidator.ValidateAsync(microsoftToken);

        if (string.IsNullOrWhiteSpace(microsoftUser.Email))
        {
            _logger.LogWarning("Microsoft token validated but email claim is missing.");
            throw new UnauthorizedAccessException("Email claim not found in token.");
        }

        if (!IsDatabaseConfigured())
        {
            return CreateDevelopmentLoginResponse(microsoftUser);
        }

        try
        {
            var normalizedEmail = microsoftUser.Email.Trim().ToLower();

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    (!string.IsNullOrEmpty(microsoftUser.ObjectId) &&
                    u.EntraObjectId == microsoftUser.ObjectId)
                    || u.Email.ToLower() == normalizedEmail);

            if (user is null)
            {
                _logger.LogWarning("Login attempt for unregistered email: {Email}", microsoftUser.Email);
                throw new KeyNotFoundException($"User with email '{microsoftUser.Email}' is not registered.");
            }

            var token = _jwtTokenService.GenerateToken(user);
var expiresAt = _jwtTokenService.GetTokenExpiry();

try
{
    await _auditLogService.LogLoginAsync(user.UserId, ipAddress);
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Failed to write login audit.");
}

_logger.LogInformation(
    "User {UserId} logged in successfully from IP {IPAddress}.",
    user.UserId,
    ipAddress);

return new LoginResponseDto
{
    Token = token,
    Role = user.Role.ToString(),
    Email = user.Email,
    Name = user.Name,
    ExpiresAt = expiresAt
};
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Database unavailable during login for {Email}. Falling back to development session.", microsoftUser.Email);
            return CreateDevelopmentLoginResponse(microsoftUser);
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "SQL login failed during authentication for {Email}. Falling back to development session.", microsoftUser.Email);
            return CreateDevelopmentLoginResponse(microsoftUser);
        }

    }

    private LoginResponseDto CreateDevelopmentLoginResponse(MicrosoftUserInfo microsoftUser)
    {
        var devUser = CreateDevUser(microsoftUser);
        DevUsers[devUser.UserId] = devUser;

        var devToken = _jwtTokenService.GenerateToken(devUser);
        var devExpiresAt = _jwtTokenService.GetTokenExpiry();

        _logger.LogWarning(
            "Issued development session for verified Microsoft user {Email}.",
            microsoftUser.Email);

        return new LoginResponseDto
        {
            Token = devToken,
            Role = devUser.Role.ToString(),
            Email = devUser.Email,
            Name = devUser.Name,
            ExpiresAt = devExpiresAt
        };
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        if (!IsDatabaseConfigured())
        {
            if (!DevUsers.TryGetValue(userId, out var devUser))
            {
                _logger.LogWarning("Development session user {UserId} not found.", userId);
                throw new KeyNotFoundException($"User {userId} not found.");
            }

            return new UserDto
            {
                UserId = devUser.UserId,
                Name = devUser.Name,
                Email = devUser.Email,
                Role = devUser.Role.ToString()
            };
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null)
        {
            _logger.LogWarning("GetCurrentUser: User {UserId} not found.", userId);
            throw new KeyNotFoundException($"User {userId} not found.");
        }

        return new UserDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task LogoutAsync(Guid userId, string ipAddress)
    {
        if (!IsDatabaseConfigured())
        {
            DevUsers.TryRemove(userId, out _);
            _logger.LogInformation("Development session user {UserId} logged out from IP {IPAddress}.", userId, ipAddress);
            return;
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null)
        {
            _logger.LogWarning("Logout: User {UserId} not found.", userId);
            throw new KeyNotFoundException($"User {userId} not found.");
        }

        await _auditLogService.LogLogoutAsync(userId, ipAddress);
        _logger.LogInformation("User {UserId} logged out from IP {IPAddress}.", userId, ipAddress);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(Guid userId)
    {
        if (!IsDatabaseConfigured())
        {
            if (!DevUsers.TryGetValue(userId, out var devUser))
            {
                _logger.LogWarning("Refresh: Development session user {UserId} not found.", userId);
                throw new KeyNotFoundException($"User {userId} not found.");
            }

            var devToken = _jwtTokenService.GenerateToken(devUser);
            var devExpiresAt = _jwtTokenService.GetTokenExpiry();

            return new LoginResponseDto
            {
                Token = devToken,
                Role = devUser.Role.ToString(),
                Email = devUser.Email,
                Name = devUser.Name,
                ExpiresAt = devExpiresAt
            };
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null)
        {
            _logger.LogWarning("Refresh: User {UserId} not found.", userId);
            throw new KeyNotFoundException($"User {userId} not found.");
        }

        var token = _jwtTokenService.GenerateToken(user);
        var expiresAt = _jwtTokenService.GetTokenExpiry();

        _logger.LogInformation("Token refreshed for user {UserId}.", userId);

        return new LoginResponseDto
        {
            Token = token,
            Role = user.Role.ToString(),
            Email = user.Email,
            Name = user.Name,
            ExpiresAt = expiresAt
        };
    }

    private bool IsDatabaseConfigured()
    {
        return !string.IsNullOrWhiteSpace(_configuration.GetConnectionString("DefaultConnection"));
    }

    private static User CreateDevUser(MicrosoftUserInfo microsoftUser)
    {
        return new User
        {
            UserId = Guid.NewGuid(),
            Name = microsoftUser.Name,
            Email = microsoftUser.Email,
            EntraObjectId = microsoftUser.ObjectId,
            Role = UserRole.ProjectManager
        };
    }
}
