using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Certificate;

public class CertificateRevokeDto
{
    [Required]
    [MaxLength(500)]
    public string RevocationReason { get; set; } = string.Empty;
}
