using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.CustomerLabAccess;

public class CustomerLabAccessCreateDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string GrantedBy { get; set; } = string.Empty;
}
