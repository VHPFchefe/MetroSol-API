namespace MetroSol.API.DTOs.StandardCertificate;

public class StandardCertificateDto
{
    public Guid Id { get; set; }
    public Guid ReferenceStandardId { get; set; }
    public Guid? ParentCertificateId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string IssuingLab { get; set; } = string.Empty;
    public string AccreditationBody { get; set; } = string.Empty;
    public double DeclaredUncertainty { get; set; }
    public string UncertaintyUnit { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public string TraceabilityLevel { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
