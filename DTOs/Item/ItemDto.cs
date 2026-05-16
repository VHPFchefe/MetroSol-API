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
    public string Unit { get; set; } = string.Empty;
    public double? RangeMin { get; set; }
    public double? RangeMax { get; set; }
    public ItemStatus Status { get; set; }
    public DateTime? LastCalibration { get; set; }
    public DateTime? NextCalibrationDue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
