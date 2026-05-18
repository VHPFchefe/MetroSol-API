using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.AssessmentCertificate;

public class AssessmentCertificateDto
{
    public Guid Id { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public Guid PerformedById { get; set; }
    public Guid SignedById { get; set; }
    public DateTime AssessmentDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? SignedAt { get; set; }
    public CertificateStatus Status { get; set; }
    public string? QrCodeUrl { get; set; }
    public string AssessmentDataJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
