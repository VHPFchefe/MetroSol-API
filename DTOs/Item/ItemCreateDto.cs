using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Item;

public class ItemCreateDto
{
    [Required]
    public Guid ItemTypeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Tag { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Manufacturer { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Model { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    public ParameterDto? Parameter { get; set; }
    public DateTime? LastAssessment { get; set; }
    public DateTime? NextAssessmentDue { get; set; }
    public bool IsReferenceStandard { get; set; } = false;

    [MaxLength(100)]
    public string? QuantityType { get; set; }
}
