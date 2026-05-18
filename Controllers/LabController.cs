using MetroSol.API.DTOs.Lab;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LabController : ControllerBase
{
    private readonly IRepository<Lab>          _labs;
    private readonly IRepository<Organization> _orgs;

    public LabController(IRepository<Lab> labs, IRepository<Organization> orgs)
    {
        _labs = labs;
        _orgs = orgs;
    }

    // ── Claim helpers ─────────────────────────────────────────────────────────

    private Guid? GetOrgId()
    {
        var claim = User.FindFirstValue("org");
        return string.IsNullOrEmpty(claim) ? null : Guid.Parse(claim);
    }

    private bool IsAdmin() => User.IsInRole("Admin");

    // -------------------------------------------------------------------------
    // GET /api/labs
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LabDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LabDto>>> GetAll()
    {
        IEnumerable<Lab> labs;

        if (IsAdmin())
            labs = await _labs.GetAllAsync();
        else
        {
            var orgId = GetOrgId();
            if (orgId is null)
                return Ok(Enumerable.Empty<LabDto>());

            labs = await _labs.FindAsync(l => l.OrganizationId == orgId.Value);
        }

        return Ok(labs.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/labs/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LabDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LabDto>> GetById(Guid id)
    {
        var lab = await _labs.GetByIdAsync(id);
        if (lab is null)
            return NotFound(new { message = $"Lab '{id}' not found." });

        if (!IsAdmin() && GetOrgId() != lab.OrganizationId)
            return Forbid();

        return Ok(MapToDto(lab));
    }

    // -------------------------------------------------------------------------
    // POST /api/labs
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(LabDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LabDto>> Create([FromBody] LabCreateDto dto)
    {
        // Non-admins can only create labs within their own organization
        if (!IsAdmin() && GetOrgId() != dto.OrganizationId)
            return Forbid();

        var org = await _orgs.GetByIdAsync(dto.OrganizationId);
        if (org is null)
            return NotFound(new { message = $"Organization '{dto.OrganizationId}' not found." });

        var lab = new Lab
        {
            OrganizationId      = dto.OrganizationId,
            Name                = dto.Name,
            Location            = dto.Location,
            AccreditationNumber = dto.AccreditationNumber
        };

        await _labs.AddAsync(lab);
        await _labs.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = lab.Id }, MapToDto(lab));
    }

    // -------------------------------------------------------------------------
    // PUT /api/labs/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(LabDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LabDto>> Update(Guid id, [FromBody] LabUpdateDto dto)
    {
        var lab = await _labs.GetByIdAsync(id);
        if (lab is null)
            return NotFound(new { message = $"Lab '{id}' not found." });

        if (!IsAdmin() && GetOrgId() != lab.OrganizationId)
            return Forbid();

        if (dto.Name                is not null) lab.Name                = dto.Name;
        if (dto.Location            is not null) lab.Location            = dto.Location;
        if (dto.AccreditationNumber is not null) lab.AccreditationNumber = dto.AccreditationNumber;

        _labs.Update(lab);
        await _labs.SaveChangesAsync();

        return Ok(MapToDto(lab));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/labs/{id}
    // -------------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var lab = await _labs.GetByIdAsync(id);
        if (lab is null)
            return NotFound(new { message = $"Lab '{id}' not found." });

        _labs.Delete(lab);
        await _labs.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static LabDto MapToDto(Lab l) => new()
    {
        Id                  = l.Id,
        OrganizationId      = l.OrganizationId,
        Name                = l.Name,
        Location            = l.Location,
        AccreditationNumber = l.AccreditationNumber,
        CreatedAt           = l.CreatedAt,
        UpdatedAt           = l.UpdatedAt
    };
}
