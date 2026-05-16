using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.User;

public class UserUpdateDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    public UserRole? Role { get; set; }

    public Guid? OrganizationId { get; set; }
}
