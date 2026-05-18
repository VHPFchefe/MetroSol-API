using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Assessment;

public class AssessmentWorkOrderDto
{
    [Required]
    [MaxLength(100)]
    public string Number { get; set; } = string.Empty;

    public DateTime? IssuedAt { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
