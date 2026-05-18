using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Assessment;

public class AssessmentRequestorDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Role { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }
}
