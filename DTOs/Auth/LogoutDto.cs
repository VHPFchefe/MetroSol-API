using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Auth;

public class LogoutDto
{
    /// <summary>The refresh token to be revoked on logout.</summary>
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
