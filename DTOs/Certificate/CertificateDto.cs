using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.Certificate;

public class CertificateDto
{
    public Guid Id { get; set; }
    public Guid AssessmentId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public CertificateStatus Status { get; set; }
    public string QrCodeUrl { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
