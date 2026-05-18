using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.AssessmentMethod;

public class AssessmentMethodDto
{
    public Guid Id { get; set; }
    public Guid? ParentMethodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }
    public string ApplicableItemTypes { get; set; } = string.Empty;
    public string DomainConceptsJson { get; set; } = string.Empty;
    public string FormulasJson { get; set; } = string.Empty;
    public string ToleranceRulesJson { get; set; } = string.Empty;
    public string DisplayTemplate { get; set; } = string.Empty;
    public bool IsHomologating { get; set; }
    public AssessmentMethodStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
