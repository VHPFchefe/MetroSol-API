using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Lab;

public class LabCreateDto
{
    [Required]
    public Guid OrganizationId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(100)]
    public string AccreditationNumber { get; set; } = string.Empty;
}
