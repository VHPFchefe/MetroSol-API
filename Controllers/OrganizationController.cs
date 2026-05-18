using MetroSol.API.DTOs.Organization;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationController : ControllerBase
{
    private readonly IRepository<Organization> _orgs;

    public OrganizationController(IRepository<Organization> orgs)
    {
        _orgs = orgs;
    }

    // ── Claim helpers ─────────────────────────────────────────────────────────

    private Guid? GetOrgId()
    {
        var claim = User.FindFirstValue("org");
        return string.IsNullOrEmpty(claim) ? null : Guid.Parse(claim);
    }

    private bool IsAdmin() =>
        User.IsInRole("Admin");

    // -------------------------------------------------------------------------
    // GET /api/organizations
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrganizationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetAll()
    {
        IEnumerable<Organization> orgs;

        if (IsAdmin())
            orgs = await _orgs.GetAllAsync();
        else
        {
            var orgId = GetOrgId();
            if (orgId is null)
                return Ok(Enumerable.Empty<OrganizationDto>());

            orgs = await _orgs.FindAsync(o => o.Id == orgId.Value);
        }

        return Ok(orgs.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/organizations/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDto>> GetById(Guid id)
    {
        var org = await _orgs.GetByIdAsync(id);
        if (org is null)
            return NotFound(new { message = $"Organization '{id}' not found." });

        if (!IsAdmin() && GetOrgId() != id)
            return Forbid();

        return Ok(MapToDto(org));
    }

    // -------------------------------------------------------------------------
    // POST /api/organizations
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrganizationDto>> Create([FromBody] OrganizationCreateDto dto)
    {
        var existing = await _orgs.FindAsync(o => o.Name == dto.Name);
        if (existing.Any())
            return Conflict(new { message = $"An organization named '{dto.Name}' already exists." });

        var org = new Organization
        {
            Name            = dto.Name,
            Country         = dto.Country,
            City            = dto.City,
            State           = dto.State,
            Street          = dto.Street,
            BuildingNumber  = dto.BuildingNumber,
            Complement      = dto.Complement,
            PostalCode      = dto.PostalCode,
            Timezone        = dto.Timezone,
            ContactEmail    = dto.ContactEmail
        };

        await _orgs.AddAsync(org);
        await _orgs.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = org.Id }, MapToDto(org));
    }

    // -------------------------------------------------------------------------
    // PUT /api/organizations/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDto>> Update(Guid id, [FromBody] OrganizationUpdateDto dto)
    {
        var org = await _orgs.GetByIdAsync(id);
        if (org is null)
            return NotFound(new { message = $"Organization '{id}' not found." });

        if (dto.Name            is not null) org.Name           = dto.Name;
        if (dto.Country         is not null) org.Country        = dto.Country;
        if (dto.City            is not null) org.City           = dto.City;
        if (dto.State           is not null) org.State          = dto.State;
        if (dto.Street          is not null) org.Street         = dto.Street;
        if (dto.BuildingNumber  is not null) org.BuildingNumber = dto.BuildingNumber;
        if (dto.Complement      is not null) org.Complement     = dto.Complement;
        if (dto.PostalCode      is not null) org.PostalCode     = dto.PostalCode;
        if (dto.Timezone        is not null) org.Timezone       = dto.Timezone;
        if (dto.ContactEmail    is not null) org.ContactEmail   = dto.ContactEmail;

        _orgs.Update(org);
        await _orgs.SaveChangesAsync();

        return Ok(MapToDto(org));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/organizations/{id}
    // -------------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var org = await _orgs.GetByIdAsync(id);
        if (org is null)
            return NotFound(new { message = $"Organization '{id}' not found." });

        _orgs.Delete(org);
        await _orgs.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static OrganizationDto MapToDto(Organization o) => new()
    {
        Id              = o.Id,
        Name            = o.Name,
        Country         = o.Country,
        City            = o.City,
        State           = o.State,
        Street          = o.Street,
        BuildingNumber  = o.BuildingNumber,
        Complement      = o.Complement,
        PostalCode      = o.PostalCode,
        Timezone        = o.Timezone,
        ContactEmail    = o.ContactEmail,
        CreatedAt       = o.CreatedAt,
        UpdatedAt       = o.UpdatedAt
    };
}
