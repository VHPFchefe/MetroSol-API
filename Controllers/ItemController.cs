using MetroSol.API.DTOs.Item;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemController : ControllerBase
{
    private readonly IRepository<Item> _items;
    private readonly IRepository<Lab>  _labs;

    public ItemController(IRepository<Item> items, IRepository<Lab> labs)
    {
        _items = items;
        _labs  = labs;
    }

    // ── Claim helpers ─────────────────────────────────────────────────────

    /// <summary>LabId stored in the "lab" JWT claim during login.</summary>
    private Guid? GetLabId()
    {
        var labClaim = User.FindFirstValue("lab");
        return string.IsNullOrEmpty(labClaim) ? null : Guid.Parse(labClaim);
    }

    private static ObjectResult NoLabResult() =>
        new ObjectResult(new { message = "User is not linked to any lab." })
            { StatusCode = StatusCodes.Status403Forbidden };

    // -------------------------------------------------------------------------
    // GET /api/items
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll()
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var items = await _items.FindAsync(i => i.LabId == labId.Value);
        return Ok(items.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/items/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> GetById(Guid id)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var item = await _items.GetByIdAsync(id);
        if (item is null)
            return NotFound(new { message = $"Item '{id}' not found." });

        if (item.LabId != labId.Value)
            return Forbid();

        return Ok(MapToDto(item));
    }

    // -------------------------------------------------------------------------
    // POST /api/items
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> Create([FromBody] ItemCreateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var lab = await _labs.GetByIdAsync(labId.Value);
        if (lab is null)
            return NotFound(new { message = "Lab not found in database." });

        var item = new Item
        {
            LabId             = labId.Value,
            ItemTypeId        = dto.ItemTypeId,
            Tag               = dto.Tag,
            Description       = dto.Description,
            Manufacturer      = dto.Manufacturer,
            Model             = dto.Model,
            SerialNumber      = dto.SerialNumber,
            Parameters        = dto.Parameters?
                .Where(p => p is not null)
                .Select(p => new MetroSol.Core.Entities.Parameter
                {
                    Name         = p!.Name,
                    Unit         = p.Unit,
                    LowerLimit   = p.LowerLimit,
                    UpperLimit   = p.UpperLimit,
                    Resolution   = p.Resolution,
                    CustomFields = p.CustomFields
                })
                .ToList(),
            LastAssessment    = dto.LastAssessment,
            NextAssessmentDue    = dto.NextAssessmentDue,
            IsReferenceStandard  = dto.IsReferenceStandard
        };

        await _items.AddAsync(item);
        await _items.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToDto(item));
    }

    // -------------------------------------------------------------------------
    // PUT /api/items/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> Update(Guid id, [FromBody] ItemUpdateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var item = await _items.GetByIdAsync(id);
        if (item is null)
            return NotFound(new { message = $"Item '{id}' not found." });

        if (item.LabId != labId.Value)
            return Forbid();

        if (dto.Tag               is not null) item.Tag               = dto.Tag;
        if (dto.Description       is not null) item.Description       = dto.Description;
        if (dto.Manufacturer      is not null) item.Manufacturer      = dto.Manufacturer;
        if (dto.Model             is not null) item.Model             = dto.Model;
        if (dto.SerialNumber      is not null) item.SerialNumber      = dto.SerialNumber;
        if (dto.Parameters?.Count > 0)
            item.Parameters = dto.Parameters
                .Where(p => p is not null)
                .Select(p => new MetroSol.Core.Entities.Parameter
                {
                    Name         = p!.Name,
                    Unit         = p.Unit,
                    LowerLimit   = p.LowerLimit,
                    UpperLimit   = p.UpperLimit,
                    Resolution   = p.Resolution,
                    CustomFields = p.CustomFields
                })
                .ToList();
        if (dto.Status              is not null) item.Status              = dto.Status.Value;
        if (dto.LastAssessment      is not null) item.LastAssessment      = dto.LastAssessment;
        if (dto.NextAssessmentDue   is not null) item.NextAssessmentDue   = dto.NextAssessmentDue;
        if (dto.IsReferenceStandard is not null) item.IsReferenceStandard = dto.IsReferenceStandard.Value;

        _items.Update(item);
        await _items.SaveChangesAsync();

        return Ok(MapToDto(item));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/items/{id}
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

        var item = await _items.GetByIdAsync(id);
        if (item is null)
            return NotFound(new { message = $"Item '{id}' not found." });

        if (item.LabId != labId.Value)
            return Forbid();

        _items.Delete(item);
        await _items.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static ItemDto MapToDto(Item item)
    {
        return new()
        {
            Id = item.Id,
            LabId = item.LabId,
            ItemTypeId = item.ItemTypeId,
            Tag = item.Tag,
            Description = item.Description,
            Manufacturer = item.Manufacturer,
            Model = item.Model,
            SerialNumber = item.SerialNumber,
            Parameters = item.Parameters?
                .Where(p => p is not null)
                .Select(p => new ParameterDto
                {
                    Name         = p.Name,
                    Unit         = p.Unit,
                    LowerLimit   = p.LowerLimit,
                    UpperLimit   = p.UpperLimit,
                    Resolution   = p.Resolution,
                    CustomFields = p.CustomFields
                })
                .ToList() ?? [],
            Status              = item.Status,
            LastAssessment      = item.LastAssessment,
            NextAssessmentDue   = item.NextAssessmentDue,
            IsReferenceStandard = item.IsReferenceStandard,
            CreatedAt           = item.CreatedAt,
            UpdatedAt           = item.UpdatedAt,
        };
    }
}
