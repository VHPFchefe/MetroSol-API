using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.Certificate;

public class CertificateCreateDto
{
    [Required]
    public Guid AssessmentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CertificateNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Standard { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Language { get; set; } = "pt";

    [Url]
    [MaxLength(500)]
    public string QrCodeUrl { get; set; } = string.Empty;

    public CertificateStatus Status { get; set; } = CertificateStatus.Draft;

    [Required]
    public DateTime IssuedAt { get; set; }
}
