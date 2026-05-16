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

    [MaxLength(20)]
    public string? Unit { get; set; }

    public double? RangeMin { get; set; }
    public double? RangeMax { get; set; }
    public ItemStatus? Status { get; set; }
    public DateTime? LastCalibration { get; set; }
    public DateTime? NextCalibrationDue { get; set; }
}
