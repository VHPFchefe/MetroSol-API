using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.Assessment;

public class AssessmentCreateDto
{
    [Required]
    public Guid ItemId { get; set; }

    [Required]
    public Guid ReferenceStandardId { get; set; }

    [Required]
    public Guid StandardCertificateId { get; set; }

    [Required]
    public Guid MethodId { get; set; }

    [Required]
    public Guid TechnicianId { get; set; }

    public Guid? SupervisorId { get; set; }

    [Range(0, double.MaxValue)]
    public double ExpandedUncertainty { get; set; }

    [Range(1, 10)]
    public int CoverageFactor { get; set; } = 2;

    public ConformityResult? ConformityResult { get; set; }

    [MaxLength(200)]
    public string ApplicableStandard { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Language { get; set; } = "pt";

    [Required]
    public DateTime PerformedAt { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }

    public AssessmentCustomerDto? Customer { get; set; }
    public AssessmentRequestorDto? Requestor { get; set; }
    public AssessmentEnvironmentDto? EnvironmentConditions { get; set; }
    public AssessmentWorkOrderDto? WorkOrder { get; set; }
}
