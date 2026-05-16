using System.ComponentModel.DataAnnotations;
using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.CalibrationCertificate;

public class CalibrationCertificateUpdateDto
{
    [MaxLength(100)]
    public string? CertificateNumber { get; set; }

    public Guid? PerformedById { get; set; }

    public Guid? SignedById { get; set; }

    public DateTime? CalibrationDate { get; set; }

    public DateTime? DueDate { get; set; }

    public CertificateStatus? Status { get; set; }

    public string? CalibrationDataJson { get; set; }
}
