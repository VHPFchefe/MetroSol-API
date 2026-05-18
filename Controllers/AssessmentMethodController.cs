using MetroSol.API.DTOs.AssessmentMethod;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssessmentMethodController : ControllerBase
{
    private readonly IRepository<AssessmentMethod> _methods;

    public AssessmentMethodController(IRepository<AssessmentMethod> methods)
    {
        _methods = methods;
    }

    // -------------------------------------------------------------------------
    // GET /api/assessment-methods
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AssessmentMethodDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AssessmentMethodDto>>> GetAll()
    {
        var methods = await _methods.GetAllAsync();
        return Ok(methods.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/assessment-methods/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssessmentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentMethodDto>> GetById(Guid id)
    {
        var method = await _methods.GetByIdAsync(id);
        if (method is null)
            return NotFound(new { message = $"AssessmentMethod '{id}' not found." });

        return Ok(MapToDto(method));
    }

    // -------------------------------------------------------------------------
    // POST /api/assessment-methods
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(AssessmentMethodDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentMethodDto>> Create([FromBody] AssessmentMethodCreateDto dto)
    {
        if (dto.ParentMethodId.HasValue)
        {
            var parent = await _methods.GetByIdAsync(dto.ParentMethodId.Value);
            if (parent is null)
                return NotFound(new { message = $"Parent method '{dto.ParentMethodId}' not found." });
        }

        var method = new AssessmentMethod
        {
            ParentMethodId     = dto.ParentMethodId,
            Name               = dto.Name,
            Version            = dto.Version,
            ApplicableItemTypes = dto.ApplicableItemTypes,
            DomainConceptsJson = dto.DomainConceptsJson,
            FormulasJson       = dto.FormulasJson,
            ToleranceRulesJson = dto.ToleranceRulesJson,
            DisplayTemplate    = dto.DisplayTemplate,
            IsHomologating     = dto.IsHomologating
        };

        await _methods.AddAsync(method);
        await _methods.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = method.Id }, MapToDto(method));
    }

    // -------------------------------------------------------------------------
    // PUT /api/assessment-methods/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(AssessmentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentMethodDto>> Update(Guid id, [FromBody] AssessmentMethodUpdateDto dto)
    {
        var method = await _methods.GetByIdAsync(id);
        if (method is null)
            return NotFound(new { message = $"AssessmentMethod '{id}' not found." });

        if (dto.Name               is not null) method.Name               = dto.Name;
        if (dto.ApplicableItemTypes is not null) method.ApplicableItemTypes = dto.ApplicableItemTypes;
        if (dto.DomainConceptsJson is not null) method.DomainConceptsJson = dto.DomainConceptsJson;
        if (dto.FormulasJson       is not null) method.FormulasJson       = dto.FormulasJson;
        if (dto.ToleranceRulesJson is not null) method.ToleranceRulesJson = dto.ToleranceRulesJson;
        if (dto.DisplayTemplate    is not null) method.DisplayTemplate    = dto.DisplayTemplate;
        if (dto.IsHomologating     is not null) method.IsHomologating     = dto.IsHomologating.Value;
        if (dto.Status             is not null) method.Status             = dto.Status.Value;

        _methods.Update(method);
        await _methods.SaveChangesAsync();

        return Ok(MapToDto(method));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/assessment-methods/{id}
    // -------------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var method = await _methods.GetByIdAsync(id);
        if (method is null)
            return NotFound(new { message = $"AssessmentMethod '{id}' not found." });

        _methods.Delete(method);
        await _methods.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static AssessmentMethodDto MapToDto(AssessmentMethod m) => new()
    {
        Id                  = m.Id,
        ParentMethodId      = m.ParentMethodId,
        Name                = m.Name,
        Version             = m.Version,
        ApplicableItemTypes = m.ApplicableItemTypes,
        DomainConceptsJson  = m.DomainConceptsJson,
        FormulasJson        = m.FormulasJson,
        ToleranceRulesJson  = m.ToleranceRulesJson,
        DisplayTemplate     = m.DisplayTemplate,
        IsHomologating      = m.IsHomologating,
        Status              = m.Status,
        CreatedAt           = m.CreatedAt,
        UpdatedAt           = m.UpdatedAt
    };
}
