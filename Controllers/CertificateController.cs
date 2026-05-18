using MetroSol.API.DTOs.Certificate;
using MetroSol.Core.Entities;
using MetroSol.Core.Enums;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificateController : ControllerBase
{
    private readonly IRepository<Certificate> _certificates;
    private readonly IRepository<Assessment>  _assessments;

    public CertificateController(
        IRepository<Certificate> certificates,
        IRepository<Assessment>  assessments)
    {
        _certificates = certificates;
        _assessments  = assessments;
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

    // ── Guard ─────────────────────────────────────────────────────────────────
    private async Task<bool> AssessmentBelongsToLabAsync(Guid assessmentId, Guid labId)
    {
        var assessment = await _assessments.GetByIdAsync(assessmentId);
        return assessment is not null && assessment.LabId == labId;
    }

    // -------------------------------------------------------------------------
    // GET /api/certificates
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CertificateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CertificateDto>>> GetAll()
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var labAssessments = await _assessments.FindAsync(a => a.LabId == labId.Value);
        var assessmentIds  = labAssessments.Select(a => a.Id).ToHashSet();

        var certs = await _certificates.FindAsync(c => assessmentIds.Contains(c.AssessmentId));
        return Ok(certs.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/certificates/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CertificateDto>> GetById(Guid id)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var cert = await _certificates.GetByIdAsync(id);
        if (cert is null)
            return NotFound(new { message = $"Certificate '{id}' not found." });

        if (!await AssessmentBelongsToLabAsync(cert.AssessmentId, labId.Value))
            return Forbid();

        return Ok(MapToDto(cert));
    }

    // -------------------------------------------------------------------------
    // POST /api/certificates
    // Only allowed when the linked Assessment is Approved.
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CertificateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CertificateDto>> Create([FromBody] CertificateCreateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var assessment = await _assessments.GetByIdAsync(dto.AssessmentId);
        if (assessment is null || assessment.LabId != labId.Value)
            return NotFound(new { message = $"Assessment '{dto.AssessmentId}' not found in this lab." });

        if (assessment.Status != AssessmentStatus.Approved)
            return BadRequest(new { message = "A certificate can only be issued for an Approved assessment." });

        // Check one-to-one: assessment must not already have a certificate
        var existing = await _certificates.FindAsync(c => c.AssessmentId == dto.AssessmentId);
        if (existing.Any())
            return Conflict(new { message = "A certificate already exists for this assessment." });

        var cert = new Certificate
        {
            AssessmentId      = dto.AssessmentId,
            CertificateNumber = dto.CertificateNumber,
            Standard          = dto.Standard,
            Language          = dto.Language,
            QrCodeUrl         = dto.QrCodeUrl,
            Status            = dto.Status,
            IssuedAt          = dto.IssuedAt
        };

        await _certificates.AddAsync(cert);
        await _certificates.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cert.Id }, MapToDto(cert));
    }

    // -------------------------------------------------------------------------
    // PATCH /api/certificates/{id}/revoke
    // -------------------------------------------------------------------------
    [HttpPatch("{id:guid}/revoke")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CertificateDto>> Revoke(Guid id, [FromBody] CertificateRevokeDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var cert = await _certificates.GetByIdAsync(id);
        if (cert is null)
            return NotFound(new { message = $"Certificate '{id}' not found." });

        if (!await AssessmentBelongsToLabAsync(cert.AssessmentId, labId.Value))
            return Forbid();

        if (cert.Status == CertificateStatus.Revoked)
            return BadRequest(new { message = "Certificate is already revoked." });

        cert.Status           = CertificateStatus.Revoked;
        cert.RevokedAt        = DateTime.UtcNow;
        cert.RevocationReason = dto.RevocationReason;

        _certificates.Update(cert);
        await _certificates.SaveChangesAsync();

        return Ok(MapToDto(cert));
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static CertificateDto MapToDto(Certificate c) => new()
    {
        Id                = c.Id,
        AssessmentId      = c.AssessmentId,
        CertificateNumber = c.CertificateNumber,
        Standard          = c.Standard,
        Language          = c.Language,
        Status            = c.Status,
        QrCodeUrl         = c.QrCodeUrl,
        IssuedAt          = c.IssuedAt,
        RevokedAt         = c.RevokedAt,
        RevocationReason  = c.RevocationReason,
        CreatedAt         = c.CreatedAt,
        UpdatedAt         = c.UpdatedAt
    };
}
