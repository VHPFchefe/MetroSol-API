using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.Item;

public class ItemDto
{
    public Guid Id { get; set; }
    public Guid LabId { get; set; }
    public Guid ItemTypeId { get; set; }
    public string Tag { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public ParameterDto? Parameter { get; set; }
    public ItemStatus Status { get; set; }
    public DateTime? LastAssessment { get; set; }
    public DateTime? NextAssessmentDue { get; set; }
    public bool IsReferenceStandard { get; set; }
    public string? QuantityType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
