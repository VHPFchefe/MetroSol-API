using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Lab;

public class LabUpdateDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? AccreditationNumber { get; set; }
}
