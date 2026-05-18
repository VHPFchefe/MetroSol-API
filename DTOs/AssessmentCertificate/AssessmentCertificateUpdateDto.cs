using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.AssessmentCertificate;

public class AssessmentCertificateUpdateDto
{
    [MaxLength(100)]
    public string? CertificateNumber { get; set; }

    public Guid? PerformedById { get; set; }

    public Guid? SignedById { get; set; }

    public DateTime? AssessmentDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? SignedAt { get; set; }

    public CertificateStatus? Status { get; set; }

    [Url]
    [MaxLength(500)]
    public string? QrCodeUrl { get; set; }

    public string? AssessmentDataJson { get; set; }
}
