using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.AssessmentCertificate;

public class AssessmentCertificateCreateDto
{
    [Required]
    [MaxLength(100)]
    public string CertificateNumber { get; set; } = string.Empty;

    [Required]
    public Guid ItemId { get; set; }

    [Required]
    public Guid PerformedById { get; set; }

    [Required]
    public Guid SignedById { get; set; }

    [Required]
    public DateTime AssessmentDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime? SignedAt { get; set; }

    public CertificateStatus Status { get; set; } = CertificateStatus.Draft;

    public string AssessmentDataJson { get; set; } = string.Empty;
}
