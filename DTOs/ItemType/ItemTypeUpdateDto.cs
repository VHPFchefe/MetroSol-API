using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.ItemType;

public class ItemTypeUpdateDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    public string? SchemaJson { get; set; }
}
