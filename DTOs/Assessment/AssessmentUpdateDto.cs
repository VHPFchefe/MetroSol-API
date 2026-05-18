using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.Assessment;

public class AssessmentUpdateDto
{
    public Guid? TechnicianId { get; set; }
    public Guid? SupervisorId { get; set; }
    public AssessmentStatus? Status { get; set; }

    [Range(0, double.MaxValue)]
    public double? ExpandedUncertainty { get; set; }

    [Range(1, 10)]
    public int? CoverageFactor { get; set; }

    public ConformityResult? ConformityResult { get; set; }

    [MaxLength(200)]
    public string? ApplicableStandard { get; set; }

    [MaxLength(10)]
    public string? Language { get; set; }

    [MaxLength(1000)]
    public string? RejectionComment { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }

    public AssessmentCustomerDto? Customer { get; set; }
    public AssessmentRequestorDto? Requestor { get; set; }
    public AssessmentEnvironmentDto? EnvironmentConditions { get; set; }
    public AssessmentWorkOrderDto? WorkOrder { get; set; }
}
