using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.Assessment;

public class AssessmentDto
{
    public Guid Id { get; set; }
    public Guid LabId { get; set; }
    public Guid ItemId { get; set; }
    public Guid ReferenceStandardId { get; set; }
    public Guid StandardCertificateId { get; set; }
    public Guid MethodId { get; set; }
    public Guid TechnicianId { get; set; }
    public Guid? SupervisorId { get; set; }
    public AssessmentStatus Status { get; set; }
    public double ExpandedUncertainty { get; set; }
    public int CoverageFactor { get; set; }
    public ConformityResult? ConformityResult { get; set; }
    public string ApplicableStandard { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string? RejectionComment { get; set; }
    public DateTime PerformedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public AssessmentCustomerDto? Customer { get; set; }
    public AssessmentRequestorDto? Requestor { get; set; }
    public AssessmentEnvironmentDto? EnvironmentConditions { get; set; }
    public AssessmentWorkOrderDto? WorkOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
