using MetroSol.API.DTOs.StandardCertificate;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StandardCertificateController : ControllerBase
{
    private readonly IRepository<StandardCertificate> _certs;
    private readonly IRepository<Item>                _items;

    public StandardCertificateController(
        IRepository<StandardCertificate> certs,
        IRepository<Item>                items)
    {
        _certs = certs;
        _items = items;
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

    // ── Guard: verify the reference standard's item belongs to this lab ───────
    private async Task<bool> RefStdBelongsToLabAsync(Guid referenceStandardId, Guid labId)
    {
        var item = await _items.GetByIdAsync(referenceStandardId);
        return item is not null && item.LabId == labId && item.IsReferenceStandard;
    }

    // -------------------------------------------------------------------------
    // GET /api/standard-certificates
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StandardCertificateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<StandardCertificateDto>>> GetAll()
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        // Filter: only certificates whose reference standard belongs to this lab
        var allItems = await _items.FindAsync(i => i.LabId == labId.Value && i.IsReferenceStandard);
        var refStdIds = allItems.Select(i => i.Id).ToHashSet();

        var certs = await _certs.FindAsync(c => refStdIds.Contains(c.ReferenceStandardId));
        return Ok(certs.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/standard-certificates/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StandardCertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StandardCertificateDto>> GetById(Guid id)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var cert = await _certs.GetByIdAsync(id);
        if (cert is null)
            return NotFound(new { message = $"StandardCertificate '{id}' not found." });

        if (!await RefStdBelongsToLabAsync(cert.ReferenceStandardId, labId.Value))
            return Forbid();

        return Ok(MapToDto(cert));
    }

    // -------------------------------------------------------------------------
    // POST /api/standard-certificates
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(typeof(StandardCertificateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StandardCertificateDto>> Create([FromBody] StandardCertificateCreateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        if (!await RefStdBelongsToLabAsync(dto.ReferenceStandardId, labId.Value))
            return NotFound(new { message = $"Reference standard '{dto.ReferenceStandardId}' not found in this lab." });

        if (dto.ValidUntil <= dto.ValidFrom)
            return BadRequest(new { message = "ValidUntil must be after ValidFrom." });

        if (dto.ParentCertificateId.HasValue)
        {
            var parent = await _certs.GetByIdAsync(dto.ParentCertificateId.Value);
            if (parent is null)
                return NotFound(new { message = $"Parent certificate '{dto.ParentCertificateId}' not found." });
        }

        var cert = new StandardCertificate
        {
            ReferenceStandardId = dto.ReferenceStandardId,
            ParentCertificateId = dto.ParentCertificateId,
            CertificateNumber   = dto.CertificateNumber,
            IssuingLab          = dto.IssuingLab,
            AccreditationBody   = dto.AccreditationBody,
            DeclaredUncertainty = dto.DeclaredUncertainty,
            UncertaintyUnit     = dto.UncertaintyUnit,
            ValidFrom           = dto.ValidFrom,
            ValidUntil          = dto.ValidUntil,
            TraceabilityLevel   = dto.TraceabilityLevel,
            IsActive            = dto.IsActive
        };

        await _certs.AddAsync(cert);
        await _certs.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cert.Id }, MapToDto(cert));
    }

    // -------------------------------------------------------------------------
    // PUT /api/standard-certificates/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(typeof(StandardCertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StandardCertificateDto>> Update(Guid id, [FromBody] StandardCertificateUpdateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var cert = await _certs.GetByIdAsync(id);
        if (cert is null)
            return NotFound(new { message = $"StandardCertificate '{id}' not found." });

        if (!await RefStdBelongsToLabAsync(cert.ReferenceStandardId, labId.Value))
            return Forbid();

        if (dto.CertificateNumber   is not null) cert.CertificateNumber   = dto.CertificateNumber;
        if (dto.IssuingLab          is not null) cert.IssuingLab          = dto.IssuingLab;
        if (dto.AccreditationBody   is not null) cert.AccreditationBody   = dto.AccreditationBody;
        if (dto.DeclaredUncertainty is not null) cert.DeclaredUncertainty = dto.DeclaredUncertainty.Value;
        if (dto.UncertaintyUnit     is not null) cert.UncertaintyUnit     = dto.UncertaintyUnit;
        if (dto.ValidFrom           is not null) cert.ValidFrom           = dto.ValidFrom.Value;
        if (dto.ValidUntil          is not null) cert.ValidUntil          = dto.ValidUntil.Value;
        if (dto.TraceabilityLevel   is not null) cert.TraceabilityLevel   = dto.TraceabilityLevel;
        if (dto.IsActive            is not null) cert.IsActive            = dto.IsActive.Value;

        // Guard: after potential update, ValidUntil must still be after ValidFrom
        if (cert.ValidUntil <= cert.ValidFrom)
            return BadRequest(new { message = "ValidUntil must be after ValidFrom." });

        _certs.Update(cert);
        await _certs.SaveChangesAsync();

        return Ok(MapToDto(cert));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/standard-certificates/{id}
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

        var cert = await _certs.GetByIdAsync(id);
        if (cert is null)
            return NotFound(new { message = $"StandardCertificate '{id}' not found." });

        if (!await RefStdBelongsToLabAsync(cert.ReferenceStandardId, labId.Value))
            return Forbid();

        _certs.Delete(cert);
        await _certs.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static StandardCertificateDto MapToDto(StandardCertificate c) => new()
    {
        Id                  = c.Id,
        ReferenceStandardId = c.ReferenceStandardId,
        ParentCertificateId = c.ParentCertificateId,
        CertificateNumber   = c.CertificateNumber,
        IssuingLab          = c.IssuingLab,
        AccreditationBody   = c.AccreditationBody,
        DeclaredUncertainty = c.DeclaredUncertainty,
        UncertaintyUnit     = c.UncertaintyUnit,
        ValidFrom           = c.ValidFrom,
        ValidUntil          = c.ValidUntil,
        TraceabilityLevel   = c.TraceabilityLevel,
        IsActive            = c.IsActive,
        CreatedAt           = c.CreatedAt,
        UpdatedAt           = c.UpdatedAt
    };
}
