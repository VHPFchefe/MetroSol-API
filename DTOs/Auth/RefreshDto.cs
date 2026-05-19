using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Auth;

public class RefreshDto
{
    /// <summary>The opaque refresh token returned by login or register.</summary>
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
