using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.CalibrationCertificate;

public class CalibrationCertificateDto
{
    public Guid Id { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public Guid PerformedById { get; set; }
    public Guid SignedById { get; set; }
    public DateTime CalibrationDate { get; set; }
    public DateTime DueDate { get; set; }
    public CertificateStatus Status { get; set; }
    public string CalibrationDataJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
