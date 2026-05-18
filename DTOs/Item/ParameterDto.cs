using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Item;

public class ParameterDto
{
    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [Required]
    public double LowerLimit { get; set; }

    [Required]
    public double UpperLimit { get; set; }

    public double? Resolution { get; set; }

    /// <summary>
    /// Optional user-defined fields. Any valid JSON value type is accepted
    /// (string, number, boolean). Example: { "AccuracyClass": "0.1", "Medium": "Gas" }
    /// </summary>
    public Dictionary<string, object?> CustomFields { get; set; } = new();
}
