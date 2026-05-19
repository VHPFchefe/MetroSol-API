namespace MetroSol.API.DTOs.AuditLog;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? AssessmentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ChangesJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
