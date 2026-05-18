using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.ItemType;

public class ItemTypeCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string SchemaJson { get; set; } = string.Empty;
}
