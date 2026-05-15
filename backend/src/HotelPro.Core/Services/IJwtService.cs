using System.Security.Claims;

namespace HotelPro.Core.Services;

public interface IJwtService
{
    (string token, DateTime expiresAt) GenerateAccessToken(Guid employeeId, string email, string role, string firstName, string lastName);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    bool VerifyPassword(string password, string passwordHash);
    string HashPassword(string password);
}
