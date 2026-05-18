using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.AssessmentMethod;

public class AssessmentMethodCreateDto
{
    public Guid? ParentMethodId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Version { get; set; } = 1;

    [MaxLength(500)]
    public string ApplicableItemTypes { get; set; } = string.Empty;

    public string DomainConceptsJson { get; set; } = string.Empty;
    public string FormulasJson { get; set; } = string.Empty;
    public string ToleranceRulesJson { get; set; } = string.Empty;
    public string DisplayTemplate { get; set; } = string.Empty;
    public bool IsHomologating { get; set; } = true;
}
