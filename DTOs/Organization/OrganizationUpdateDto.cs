using System.ComponentModel.DataAnnotations;

namespace MetroSol.API.DTOs.Organization;

public class OrganizationUpdateDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(200)]
    public string? Street { get; set; }

    [MaxLength(20)]
    public string? BuildingNumber { get; set; }

    [MaxLength(100)]
    public string? Complement { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(50)]
    public string? Timezone { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? ContactEmail { get; set; }
}
