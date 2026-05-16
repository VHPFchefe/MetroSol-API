using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.CalibrationCertificate;

public class CalibrationCertificateCreateDto
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
    public DateTime CalibrationDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public CertificateStatus Status { get; set; } = CertificateStatus.Draft;

    public string CalibrationDataJson { get; set; } = string.Empty;
}
