using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.AssessmentPoint;

public class AssessmentPointDto
{
    public Guid Id { get; set; }
    public Guid AssessmentId { get; set; }
    public string ConceptName { get; set; } = string.Empty;
    public double NominalValue { get; set; }
    public double MeasuredValue { get; set; }
    public double Error { get; set; }
    public double Correction { get; set; }
    public int PointOrder { get; set; }
    public InputSource InputSource { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
