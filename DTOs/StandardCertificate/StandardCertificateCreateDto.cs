using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.StandardCertificate;

public class StandardCertificateCreateDto
{
    [Required]
    public Guid ReferenceStandardId { get; set; }

    public Guid? ParentCertificateId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CertificateNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string IssuingLab { get; set; } = string.Empty;

    [MaxLength(200)]
    public string AccreditationBody { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public double DeclaredUncertainty { get; set; }

    [Required]
    [MaxLength(50)]
    public string UncertaintyUnit { get; set; } = string.Empty;

    [Required]
    public DateTime ValidFrom { get; set; }

    [Required]
    public DateTime ValidUntil { get; set; }

    [MaxLength(200)]
    public string TraceabilityLevel { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
