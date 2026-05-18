using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.AssessmentMethod;

public class AssessmentMethodUpdateDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? ApplicableItemTypes { get; set; }

    public string? DomainConceptsJson { get; set; }
    public string? FormulasJson { get; set; }
    public string? ToleranceRulesJson { get; set; }
    public string? DisplayTemplate { get; set; }
    public bool? IsHomologating { get; set; }
    public AssessmentMethodStatus? Status { get; set; }
}
