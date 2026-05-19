using MetroSol.Core.Enums;

namespace MetroSol.API.DTOs.BillingEvent;

public class BillingEventDto
{
    public Guid Id { get; set; }
    public Guid CertificateId { get; set; }
    public Guid OrganizationId { get; set; }
    public BillingEventType EventType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Edition { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
