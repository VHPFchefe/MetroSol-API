namespace MetroSol.API.DTOs.CustomerLabAccess;

public class CustomerLabAccessDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LabId { get; set; }
    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
