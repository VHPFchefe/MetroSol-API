using MetroSol.API.DTOs.AssessmentPoint;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssessmentPointController : ControllerBase
{
    private readonly IRepository<AssessmentPoint> _points;
    private readonly IRepository<Assessment>      _assessments;

    public AssessmentPointController(
        IRepository<AssessmentPoint> points,
        IRepository<Assessment>      assessments)
    {
        _points      = points;
        _assessments = assessments;
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

    // ── Guard: verify the assessment belongs to the user's lab ────────────────
    private async Task<(Assessment? assessment, ActionResult? error)> ResolveAssessmentAsync(Guid assessmentId)
    {
        var labId = GetLabId();
        if (labId is null)
            return (null, NoLabResult());

        var assessment = await _assessments.GetByIdAsync(assessmentId);
        if (assessment is null)
            return (null, NotFound(new { message = $"Assessment '{assessmentId}' not found." }));

        if (assessment.LabId != labId.Value)
            return (null, Forbid());

        return (assessment, null);
    }

    // -------------------------------------------------------------------------
    // GET /api/assessment-points?assessmentId={guid}
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AssessmentPointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AssessmentPointDto>>> GetByAssessment([FromQuery] Guid assessmentId)
    {
        var (_, error) = await ResolveAssessmentAsync(assessmentId);
        if (error is not null) return error;

        var points = await _points.FindAsync(p => p.AssessmentId == assessmentId);
        return Ok(points.OrderBy(p => p.PointOrder).Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/assessment-points/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssessmentPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentPointDto>> GetById(Guid id)
    {
        var point = await _points.GetByIdAsync(id);
        if (point is null)
            return NotFound(new { message = $"AssessmentPoint '{id}' not found." });

        var (_, error) = await ResolveAssessmentAsync(point.AssessmentId);
        if (error is not null) return error;

        return Ok(MapToDto(point));
    }

    // -------------------------------------------------------------------------
    // POST /api/assessment-points
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(typeof(AssessmentPointDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentPointDto>> Create([FromBody] AssessmentPointCreateDto dto)
    {
        var (_, error) = await ResolveAssessmentAsync(dto.AssessmentId);
        if (error is not null) return error;

        var point = new AssessmentPoint
        {
            AssessmentId  = dto.AssessmentId,
            ConceptName   = dto.ConceptName,
            NominalValue  = dto.NominalValue,
            MeasuredValue = dto.MeasuredValue,
            Error         = dto.Error,
            Correction    = dto.Correction,
            PointOrder    = dto.PointOrder,
            InputSource   = dto.InputSource
        };

        await _points.AddAsync(point);
        await _points.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = point.Id }, MapToDto(point));
    }

    // -------------------------------------------------------------------------
    // PUT /api/assessment-points/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(typeof(AssessmentPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentPointDto>> Update(Guid id, [FromBody] AssessmentPointUpdateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var point = await _points.GetByIdAsync(id);
        if (point is null)
            return NotFound(new { message = $"AssessmentPoint '{id}' not found." });

        var (_, error) = await ResolveAssessmentAsync(point.AssessmentId);
        if (error is not null) return error;

        if (dto.ConceptName   is not null) point.ConceptName   = dto.ConceptName;
        if (dto.NominalValue  is not null) point.NominalValue  = dto.NominalValue.Value;
        if (dto.MeasuredValue is not null) point.MeasuredValue = dto.MeasuredValue.Value;
        if (dto.Error         is not null) point.Error         = dto.Error.Value;
        if (dto.Correction    is not null) point.Correction    = dto.Correction.Value;
        if (dto.PointOrder    is not null) point.PointOrder    = dto.PointOrder.Value;
        if (dto.InputSource   is not null) point.InputSource   = dto.InputSource.Value;

        _points.Update(point);
        await _points.SaveChangesAsync();

        return Ok(MapToDto(point));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/assessment-points/{id}
    // -------------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var point = await _points.GetByIdAsync(id);
        if (point is null)
            return NotFound(new { message = $"AssessmentPoint '{id}' not found." });

        var (_, error) = await ResolveAssessmentAsync(point.AssessmentId);
        if (error is not null) return error;

        _points.Delete(point);
        await _points.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static AssessmentPointDto MapToDto(AssessmentPoint p) => new()
    {
        Id            = p.Id,
        AssessmentId  = p.AssessmentId,
        ConceptName   = p.ConceptName,
        NominalValue  = p.NominalValue,
        MeasuredValue = p.MeasuredValue,
        Error         = p.Error,
        Correction    = p.Correction,
        PointOrder    = p.PointOrder,
        InputSource   = p.InputSource,
        CreatedAt     = p.CreatedAt,
        UpdatedAt     = p.UpdatedAt
    };
}
