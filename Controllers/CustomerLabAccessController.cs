using MetroSol.API.DTOs.CustomerLabAccess;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerLabAccessController : ControllerBase
{
    private readonly IRepository<CustomerLabAccess> _accesses;
    private readonly IRepository<Lab>               _labs;
    private readonly IRepository<User>              _users;

    public CustomerLabAccessController(
        IRepository<CustomerLabAccess> accesses,
        IRepository<Lab>               labs,
        IRepository<User>              users)
    {
        _accesses = accesses;
        _labs     = labs;
        _users    = users;
    }

    // ── Claim helpers ─────────────────────────────────────────────────────────

    private Guid? GetLabId()
    {
        var claim = User.FindFirstValue("lab");
        return string.IsNullOrEmpty(claim) ? null : Guid.Parse(claim);
    }

    private static ObjectResult NoLabResult() =>
        new ObjectResult(new { message = "User is not linked to any lab." })
            { StatusCode = StatusCodes.Status403Forbidden };

    // -------------------------------------------------------------------------
    // GET /api/customer-lab-accesses
    // Returns all access grants for the caller's lab.
    // -------------------------------------------------------------------------
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(IEnumerable<CustomerLabAccessDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CustomerLabAccessDto>>> GetAll()
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var accesses = await _accesses.FindAsync(a => a.LabId == labId.Value);
        return Ok(accesses.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/customer-lab-accesses/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CustomerLabAccessDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerLabAccessDto>> GetById(Guid id)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var access = await _accesses.GetByIdAsync(id);
        if (access is null)
            return NotFound(new { message = $"CustomerLabAccess '{id}' not found." });

        if (access.LabId != labId.Value)
            return Forbid();

        return Ok(MapToDto(access));
    }

    // -------------------------------------------------------------------------
    // POST /api/customer-lab-accesses
    // Grants a user access to the caller's lab.
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CustomerLabAccessDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerLabAccessDto>> Create([FromBody] CustomerLabAccessCreateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var lab = await _labs.GetByIdAsync(labId.Value);
        if (lab is null)
            return NotFound(new { message = $"Lab '{labId}' not found." });

        var user = await _users.GetByIdAsync(dto.UserId);
        if (user is null)
            return NotFound(new { message = $"User '{dto.UserId}' not found." });

        // Prevent duplicate grants
        var existing = await _accesses.FindAsync(
            a => a.UserId == dto.UserId && a.LabId == labId.Value);
        if (existing.Any())
            return Conflict(new { message = "This user already has access to the lab." });

        var access = new CustomerLabAccess
        {
            UserId    = dto.UserId,
            LabId     = labId.Value,
            GrantedBy = dto.GrantedBy,
            GrantedAt = DateTime.UtcNow
        };

        await _accesses.AddAsync(access);
        await _accesses.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = access.Id }, MapToDto(access));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/customer-lab-accesses/{id}
    // Revokes a user's access to the lab.
    // -------------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var access = await _accesses.GetByIdAsync(id);
        if (access is null)
            return NotFound(new { message = $"CustomerLabAccess '{id}' not found." });

        if (access.LabId != labId.Value)
            return Forbid();

        _accesses.Delete(access);
        await _accesses.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static CustomerLabAccessDto MapToDto(CustomerLabAccess a) => new()
    {
        Id        = a.Id,
        UserId    = a.UserId,
        LabId     = a.LabId,
        GrantedAt = a.GrantedAt,
        GrantedBy = a.GrantedBy,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
