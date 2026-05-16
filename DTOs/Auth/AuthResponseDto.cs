namespace MetroSol.API.DTOs.Auth;

public class AuthResponseDto
{
    /// <summary>JWT Bearer token. Send in the header: Authorization: Bearer {AccessToken}</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Token lifetime in seconds.</summary>
    public int ExpiresIn { get; set; }

    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Role as string (Admin, Manager, Technician, Customer).</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Null for users not yet linked to an organization..</summary>
    public Guid? OrganizationId { get; set; }
}
