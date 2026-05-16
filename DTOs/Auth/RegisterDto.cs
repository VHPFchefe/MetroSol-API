using MetroSol.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Minimum 8 characters. Stored as a BCrypt hash — never in plain text..</summary>
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    /// <summary>
    /// Optional during initial registration.
    /// A user without an organization can only be linked to an organization later by the Admin.
    /// </summary>
    public Guid? OrganizationId { get; set; }
}
