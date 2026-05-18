using MetroSol.API.DTOs.ItemType;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemTypeController : ControllerBase
{
    private readonly IRepository<ItemType> _itemTypes;

    public ItemTypeController(IRepository<ItemType> itemTypes)
    {
        _itemTypes = itemTypes;
    }

    // -------------------------------------------------------------------------
    // GET /api/item-types
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ItemTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ItemTypeDto>>> GetAll()
    {
        var types = await _itemTypes.GetAllAsync();
        return Ok(types.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/item-types/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemTypeDto>> GetById(Guid id)
    {
        var type = await _itemTypes.GetByIdAsync(id);
        if (type is null)
            return NotFound(new { message = $"ItemType '{id}' not found." });

        return Ok(MapToDto(type));
    }

    // -------------------------------------------------------------------------
    // POST /api/item-types
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ItemTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ItemTypeDto>> Create([FromBody] ItemTypeCreateDto dto)
    {
        var existing = await _itemTypes.FindAsync(t => t.Name == dto.Name);
        if (existing.Any())
            return Conflict(new { message = $"An ItemType named '{dto.Name}' already exists." });

        var type = new ItemType
        {
            Name       = dto.Name,
            SchemaJson = dto.SchemaJson
        };

        await _itemTypes.AddAsync(type);
        await _itemTypes.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = type.Id }, MapToDto(type));
    }

    // -------------------------------------------------------------------------
    // PUT /api/item-types/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ItemTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemTypeDto>> Update(Guid id, [FromBody] ItemTypeUpdateDto dto)
    {
        var type = await _itemTypes.GetByIdAsync(id);
        if (type is null)
            return NotFound(new { message = $"ItemType '{id}' not found." });

        if (dto.Name       is not null) type.Name       = dto.Name;
        if (dto.SchemaJson is not null) type.SchemaJson = dto.SchemaJson;

        _itemTypes.Update(type);
        await _itemTypes.SaveChangesAsync();

        return Ok(MapToDto(type));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/item-types/{id}
    // -------------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var type = await _itemTypes.GetByIdAsync(id);
        if (type is null)
            return NotFound(new { message = $"ItemType '{id}' not found." });

        _itemTypes.Delete(type);
        await _itemTypes.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static ItemTypeDto MapToDto(ItemType t) => new()
    {
        Id         = t.Id,
        Name       = t.Name,
        SchemaJson = t.SchemaJson,
        CreatedAt  = t.CreatedAt,
        UpdatedAt  = t.UpdatedAt
    };
}
