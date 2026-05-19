using MetroSol.API.DTOs.AuditLog;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class AuditLogController : ControllerBase
{
    private readonly IRepository<AuditLog>   _logs;
    private readonly IRepository<Assessment> _assessments;

    public AuditLogController(
        IRepository<AuditLog>   logs,
        IRepository<Assessment> assessments)
    {
        _logs        = logs;
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

    // -------------------------------------------------------------------------
    // GET /api/audit-logs
    // Returns all audit logs for assessments in the caller's lab.
    // Optional filter: ?assessmentId={guid}
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetAll(
        [FromQuery] Guid? assessmentId = null)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        IEnumerable<AuditLog> logs;

        if (assessmentId.HasValue)
        {
            // Verify the assessment belongs to this lab before returning its logs
            var assessment = await _assessments.GetByIdAsync(assessmentId.Value);
            if (assessment is null || assessment.LabId != labId.Value)
                return NotFound(new { message = $"Assessment '{assessmentId}' not found in this lab." });

            logs = await _logs.FindAsync(l => l.AssessmentId == assessmentId.Value);
        }
        else
        {
            // Return all logs for every assessment in this lab
            var labAssessments = await _assessments.FindAsync(a => a.LabId == labId.Value);
            var assessmentIds  = labAssessments.Select(a => a.Id).ToHashSet();

            logs = await _logs.FindAsync(l =>
                l.AssessmentId.HasValue && assessmentIds.Contains(l.AssessmentId.Value));
        }

        return Ok(logs.OrderByDescending(l => l.CreatedAt).Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/audit-logs/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditLogDto>> GetById(Guid id)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var log = await _logs.GetByIdAsync(id);
        if (log is null)
            return NotFound(new { message = $"AuditLog '{id}' not found." });

        // Verify ownership via the linked assessment
        if (log.AssessmentId.HasValue)
        {
            var assessment = await _assessments.GetByIdAsync(log.AssessmentId.Value);
            if (assessment is null || assessment.LabId != labId.Value)
                return Forbid();
        }

        return Ok(MapToDto(log));
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static AuditLogDto MapToDto(AuditLog l) => new()
    {
        Id           = l.Id,
        UserId       = l.UserId,
        AssessmentId = l.AssessmentId,
        Action       = l.Action,
        ChangesJson  = l.ChangesJson,
        CreatedAt    = l.CreatedAt
    };
}
