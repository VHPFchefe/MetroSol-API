using MetroSol.API.DTOs.BillingEvent;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BillingEventController : ControllerBase
{
    private readonly IRepository<BillingEvent>  _events;
    private readonly IRepository<Organization>  _orgs;

    public BillingEventController(
        IRepository<BillingEvent> events,
        IRepository<Organization> orgs)
    {
        _events = events;
        _orgs   = orgs;
    }

    // ── Claim helpers ─────────────────────────────────────────────────────────

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirstValue("org");
        return string.IsNullOrEmpty(claim) ? null : Guid.Parse(claim);
    }

    private static ObjectResult NoOrgResult() =>
        new ObjectResult(new { message = "User is not linked to any organization." })
            { StatusCode = StatusCodes.Status403Forbidden };

    // -------------------------------------------------------------------------
    // GET /api/billing-events
    // Returns all billing events for the caller's organization.
    // Optional filters: ?eventType={int}  ?certificateId={guid}
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BillingEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<BillingEventDto>>> GetAll(
        [FromQuery] int?  eventType     = null,
        [FromQuery] Guid? certificateId = null)
    {
        var orgId = GetOrganizationId();
        if (orgId is null) return NoOrgResult();

        var events = await _events.FindAsync(e => e.OrganizationId == orgId.Value);

        if (eventType.HasValue)
            events = events.Where(e => (int)e.EventType == eventType.Value);

        if (certificateId.HasValue)
            events = events.Where(e => e.CertificateId == certificateId.Value);

        return Ok(events.OrderByDescending(e => e.OccurredAt).Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/billing-events/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BillingEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BillingEventDto>> GetById(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId is null) return NoOrgResult();

        var billingEvent = await _events.GetByIdAsync(id);
        if (billingEvent is null)
            return NotFound(new { message = $"BillingEvent '{id}' not found." });

        if (billingEvent.OrganizationId != orgId.Value)
            return Forbid();

        return Ok(MapToDto(billingEvent));
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static BillingEventDto MapToDto(BillingEvent e) => new()
    {
        Id             = e.Id,
        CertificateId  = e.CertificateId,
        OrganizationId = e.OrganizationId,
        EventType      = e.EventType,
        Amount         = e.Amount,
        Currency       = e.Currency,
        Edition        = e.Edition,
        OccurredAt     = e.OccurredAt,
        CreatedAt      = e.CreatedAt
    };
}
