using MetroSol.Core.Entities;

namespace MetroSol.API.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a cryptographically random refresh token string.
    /// Storage and lifecycle are managed by the caller (AuthController).
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>Configured expiration window for refresh tokens (from appsettings).</summary>
    int RefreshTokenExpirationDays { get; }
}
