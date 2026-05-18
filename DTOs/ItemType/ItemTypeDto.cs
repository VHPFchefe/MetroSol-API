namespace MetroSol.API.DTOs.ItemType;

public class ItemTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SchemaJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
