namespace MetroSol.API.DTOs.Auth;

public class AuthResponseDto
{
    /// <summary>Short-lived JWT. Send in the header: Authorization: Bearer {AccessToken}</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Access token lifetime in seconds (default: 900 = 15 min).</summary>
    public int ExpiresIn { get; set; }

    /// <summary>Long-lived opaque token used to obtain a new access token via POST /auth/refresh.</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>UTC expiration of the refresh token.</summary>
    public DateTime RefreshTokenExpiresAt { get; set; }

    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Role as string (Admin, Manager, Technician, Customer).</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Null for users not yet linked to an organization.</summary>
    public Guid? OrganizationId { get; set; }
}
