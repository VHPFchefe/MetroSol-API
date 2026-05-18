using MetroSol.Core.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MetroSol.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user)
    {
        var secret     = _config["Jwt:Secret"]!;
        var issuer     = _config["Jwt:Issuer"]!;
        var audience   = _config["Jwt:Audience"]!;
        var expMinutes = int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "15");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ── Claims embedded in the token ──────────────────────────────────────────
        // "sub"  → userId  (unequivocal user identification)
        // "role" → UserRole as string  (used by [Authorize(Roles = "Admin")])
        // "org"  → OrganizationId      (multi-tenant filtering in controllers)
        // "jti"  → unique token        (prepared for future repeal)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name,  user.Name),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim("org", user.OrganizationId?.ToString() ?? string.Empty),
            new Claim("lab", user.LabId?.ToString() ?? string.Empty),
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
