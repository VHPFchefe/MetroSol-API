using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.Item;

public class ItemUpdateDto
{
    [MaxLength(100)]
    public string? Tag { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Manufacturer { get; set; }

    [MaxLength(200)]
    public string? Model { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    public List<ParameterDto?> Parameters { get; set; } = new List<ParameterDto?>();
    public ItemStatus? Status { get; set; }
    public DateTime? LastAssessment { get; set; }
    public DateTime? NextAssessmentDue { get; set; }
    public bool? IsReferenceStandard { get; set; }

    [MaxLength(100)]
    public string? QuantityType { get; set; }
}
