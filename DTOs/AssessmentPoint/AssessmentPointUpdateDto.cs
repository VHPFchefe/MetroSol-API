using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.AssessmentPoint;

public class AssessmentPointUpdateDto
{
    [MaxLength(100)]
    public string? ConceptName { get; set; }

    public double? NominalValue { get; set; }
    public double? MeasuredValue { get; set; }
    public double? Error { get; set; }
    public double? Correction { get; set; }

    [Range(0, int.MaxValue)]
    public int? PointOrder { get; set; }

    public InputSource? InputSource { get; set; }
}
