using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

namespace HotelPro.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly HotelProDbContext _dbContext;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtService jwtService, HotelProDbContext dbContext, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("login")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var employee = await _dbContext.Employees
            .FirstOrDefaultAsync(e => e.Email == request.Email, ct);

        if (employee == null || string.IsNullOrEmpty(employee.PasswordHash))
        {
            _logger.LogWarning("Login attempt for non-existent user: {Email}", request.Email);
            return Unauthorized(new { error = "InvalidCredentials", message = "Invalid email or password" });
        }

        if (!_jwtService.VerifyPassword(request.Password, employee.PasswordHash))
        {
            _logger.LogWarning("Invalid password for user: {Email}", request.Email);
            return Unauthorized(new { error = "InvalidCredentials", message = "Invalid email or password" });
        }

        if (!employee.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
            return Unauthorized(new { error = "AccountDisabled", message = "Account is disabled" });
        }

        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(
            employee.Id,
            employee.Email,
            employee.Role.ToString(),
            employee.FirstName,
            employee.LastName
        );

        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenEntity = new HotelPro.Core.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            Role = employee.Role.ToString(),
            Name = $"{employee.FirstName} {employee.LastName}"
        });
    }

    [HttpPost("refresh")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var storedToken = await _dbContext.RefreshTokens
            .Include(rt => rt.Employee)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired refresh token used");
            return Unauthorized(new { error = "InvalidRefreshToken", message = "Invalid or expired refresh token" });
        }

        if (!storedToken.Employee.IsActive)
        {
            _logger.LogWarning("Refresh token used by inactive user: {Email}", storedToken.Employee.Email);
            return Unauthorized(new { error = "AccountDisabled", message = "Account is disabled" });
        }

        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(
            storedToken.Employee.Id,
            storedToken.Employee.Email,
            storedToken.Employee.Role.ToString(),
            storedToken.Employee.FirstName,
            storedToken.Employee.LastName
        );

        storedToken.IsRevoked = true;
        storedToken.ReplacedByToken = newRefreshToken;

        var newRefreshTokenEntity = new HotelPro.Core.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            EmployeeId = storedToken.EmployeeId,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await _dbContext.SaveChangesAsync(ct);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            Role = storedToken.Employee.Role.ToString(),
            Name = $"{storedToken.Employee.FirstName} {storedToken.Employee.LastName}"
        });
    }

    [HttpPost("logout")]
    [Authorize]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (token != null)
        {
            token.IsRevoked = true;
            await _dbContext.SaveChangesAsync(ct);
        }

        return Ok(new { message = "Logged out successfully" });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
