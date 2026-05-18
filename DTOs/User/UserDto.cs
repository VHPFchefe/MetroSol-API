using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public Guid? LabId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
