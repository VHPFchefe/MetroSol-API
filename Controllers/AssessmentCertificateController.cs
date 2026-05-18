using MetroSol.API.DTOs.AssessmentCertificate;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssessmentCertificateController : ControllerBase
{
    private readonly IRepository<AssessmentCertificate> _certs;
    private readonly IRepository<Item>                  _items;

    public AssessmentCertificateController(
        IRepository<AssessmentCertificate> certs,
        IRepository<Item>                  items)
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

    // ── Guard: item must belong to this lab ───────────────────────────────────
    private async Task<bool> ItemBelongsToLabAsync(Guid itemId, Guid labId)
    {
        var item = await _items.GetByIdAsync(itemId);
        return item is not null && item.LabId == labId;
    }

    // -------------------------------------------------------------------------
    // GET /api/assessment-certificates
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AssessmentCertificateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AssessmentCertificateDto>>> GetAll()
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var labItems  = await _items.FindAsync(i => i.LabId == labId.Value);
        var itemIds   = labItems.Select(i => i.Id).ToHashSet();

        var certs = await _certs.FindAsync(c => itemIds.Contains(c.ItemId));
        return Ok(certs.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/assessment-certificates/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssessmentCertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentCertificateDto>> GetById(Guid id)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var cert = await _certs.GetByIdAsync(id);
        if (cert is null)
            return NotFound(new { message = $"AssessmentCertificate '{id}' not found." });

        if (!await ItemBelongsToLabAsync(cert.ItemId, labId.Value))
            return Forbid();

        return Ok(MapToDto(cert));
    }

    // -------------------------------------------------------------------------
    // POST /api/assessment-certificates
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(typeof(AssessmentCertificateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentCertificateDto>> Create([FromBody] AssessmentCertificateCreateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        if (!await ItemBelongsToLabAsync(dto.ItemId, labId.Value))
            return NotFound(new { message = $"Item '{dto.ItemId}' not found in this lab." });

        var cert = new AssessmentCertificate
        {
            CertificateNumber  = dto.CertificateNumber,
            ItemId             = dto.ItemId,
            PerformedById      = dto.PerformedById,
            SignedById         = dto.SignedById,
            AssessmentDate     = dto.AssessmentDate,
            DueDate            = dto.DueDate,
            SignedAt           = dto.SignedAt,
            Status             = dto.Status,
            AssessmentDataJson = dto.AssessmentDataJson
        };

        await _certs.AddAsync(cert);
        await _certs.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cert.Id }, MapToDto(cert));
    }

    // -------------------------------------------------------------------------
    // PUT /api/assessment-certificates/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(typeof(AssessmentCertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentCertificateDto>> Update(Guid id, [FromBody] AssessmentCertificateUpdateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var cert = await _certs.GetByIdAsync(id);
        if (cert is null)
            return NotFound(new { message = $"AssessmentCertificate '{id}' not found." });

        if (!await ItemBelongsToLabAsync(cert.ItemId, labId.Value))
            return Forbid();

        if (dto.CertificateNumber  is not null) cert.CertificateNumber  = dto.CertificateNumber;
        if (dto.PerformedById      is not null) cert.PerformedById      = dto.PerformedById.Value;
        if (dto.SignedById         is not null) cert.SignedById         = dto.SignedById.Value;
        if (dto.AssessmentDate     is not null) cert.AssessmentDate     = dto.AssessmentDate.Value;
        if (dto.DueDate            is not null) cert.DueDate            = dto.DueDate.Value;
        if (dto.SignedAt           is not null) cert.SignedAt           = dto.SignedAt;
        if (dto.Status             is not null) cert.Status             = dto.Status.Value;
        if (dto.QrCodeUrl          is not null) cert.QrCodeUrl          = dto.QrCodeUrl;
        if (dto.AssessmentDataJson is not null) cert.AssessmentDataJson = dto.AssessmentDataJson;

        _certs.Update(cert);
        await _certs.SaveChangesAsync();

        return Ok(MapToDto(cert));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/assessment-certificates/{id}
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
            return NotFound(new { message = $"AssessmentCertificate '{id}' not found." });

        if (!await ItemBelongsToLabAsync(cert.ItemId, labId.Value))
            return Forbid();

        _certs.Delete(cert);
        await _certs.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static AssessmentCertificateDto MapToDto(AssessmentCertificate c) => new()
    {
        Id                 = c.Id,
        CertificateNumber  = c.CertificateNumber,
        ItemId             = c.ItemId,
        PerformedById      = c.PerformedById,
        SignedById         = c.SignedById,
        AssessmentDate     = c.AssessmentDate,
        DueDate            = c.DueDate,
        SignedAt           = c.SignedAt,
        Status             = c.Status,
        QrCodeUrl          = c.QrCodeUrl,
        AssessmentDataJson = c.AssessmentDataJson,
        CreatedAt          = c.CreatedAt,
        UpdatedAt          = c.UpdatedAt
    };
}
