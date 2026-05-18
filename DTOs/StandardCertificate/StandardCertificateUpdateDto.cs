using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.StandardCertificate;

public class StandardCertificateUpdateDto
{
    [MaxLength(100)]
    public string? CertificateNumber { get; set; }

    [MaxLength(200)]
    public string? IssuingLab { get; set; }

    [MaxLength(200)]
    public string? AccreditationBody { get; set; }

    [Range(0, double.MaxValue)]
    public double? DeclaredUncertainty { get; set; }

    [MaxLength(50)]
    public string? UncertaintyUnit { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }

    [MaxLength(200)]
    public string? TraceabilityLevel { get; set; }

    public bool? IsActive { get; set; }
}
