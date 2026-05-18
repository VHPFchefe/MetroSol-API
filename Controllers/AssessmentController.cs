using MetroSol.API.DTOs.Assessment;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssessmentController : ControllerBase
{
    private readonly IRepository<Assessment>        _assessments;
    private readonly IRepository<Lab>               _labs;
    private readonly IRepository<Item>              _items;
    private readonly IRepository<AssessmentMethod>  _methods;
    private readonly IRepository<StandardCertificate> _stdCerts;

    public AssessmentController(
        IRepository<Assessment>          assessments,
        IRepository<Lab>                 labs,
        IRepository<Item>                items,
        IRepository<AssessmentMethod>    methods,
        IRepository<StandardCertificate> stdCerts)
    {
        _assessments = assessments;
        _labs        = labs;
        _items       = items;
        _methods     = methods;
        _stdCerts    = stdCerts;
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
    // GET /api/assessments
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AssessmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAll()
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var assessments = await _assessments.FindAsync(a => a.LabId == labId.Value);
        return Ok(assessments.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/assessments/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentDto>> GetById(Guid id)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var assessment = await _assessments.GetByIdAsync(id);
        if (assessment is null)
            return NotFound(new { message = $"Assessment '{id}' not found." });

        if (assessment.LabId != labId.Value)
            return Forbid();

        return Ok(MapToDto(assessment));
    }

    // -------------------------------------------------------------------------
    // POST /api/assessments
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(typeof(AssessmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentDto>> Create([FromBody] AssessmentCreateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        // Validate item belongs to this lab
        var item = await _items.GetByIdAsync(dto.ItemId);
        if (item is null || item.LabId != labId.Value)
            return NotFound(new { message = $"Item '{dto.ItemId}' not found in this lab." });

        // Validate reference standard belongs to this lab and is promoted
        var refStd = await _items.GetByIdAsync(dto.ReferenceStandardId);
        if (refStd is null || refStd.LabId != labId.Value || !refStd.IsReferenceStandard)
            return NotFound(new { message = $"Reference standard '{dto.ReferenceStandardId}' not found in this lab." });

        // Validate standard certificate exists and is active
        var stdCert = await _stdCerts.GetByIdAsync(dto.StandardCertificateId);
        if (stdCert is null || !stdCert.IsActive)
            return NotFound(new { message = $"Standard certificate '{dto.StandardCertificateId}' not found or inactive." });

        // Validate method exists
        var method = await _methods.GetByIdAsync(dto.MethodId);
        if (method is null)
            return NotFound(new { message = $"AssessmentMethod '{dto.MethodId}' not found." });

        var assessment = new Assessment
        {
            LabId                 = labId.Value,
            ItemId                = dto.ItemId,
            ReferenceStandardId   = dto.ReferenceStandardId,
            StandardCertificateId = dto.StandardCertificateId,
            MethodId              = dto.MethodId,
            TechnicianId          = dto.TechnicianId,
            SupervisorId          = dto.SupervisorId,
            ExpandedUncertainty   = dto.ExpandedUncertainty,
            CoverageFactor        = dto.CoverageFactor,
            ConformityResult      = dto.ConformityResult,
            ApplicableStandard    = dto.ApplicableStandard,
            Language              = dto.Language,
            PerformedAt           = dto.PerformedAt,
            StartDate             = dto.StartDate,
            CompletionDate        = dto.CompletionDate,
            Customer              = dto.Customer is null ? null : new Customer
            {
                Name          = dto.Customer.Name,
                ContactPerson = dto.Customer.ContactPerson,
                Email         = dto.Customer.Email,
                Phone         = dto.Customer.Phone
            },
            Requestor             = dto.Requestor is null ? null : new Requestor
            {
                Name       = dto.Requestor.Name,
                Department = dto.Requestor.Department,
                Role       = dto.Requestor.Role,
                Email      = dto.Requestor.Email
            },
            EnvironmentConditions = dto.EnvironmentConditions is null ? null : new EnvironmentConditions
            {
                InitialTemperature = dto.EnvironmentConditions.InitialTemperature,
                MiddleTemperature  = dto.EnvironmentConditions.MiddleTemperature,
                FinalTemperature   = dto.EnvironmentConditions.FinalTemperature,
                InitialHumidity    = dto.EnvironmentConditions.InitialHumidity,
                MiddleHumidity     = dto.EnvironmentConditions.MiddleHumidity,
                FinalHumidity      = dto.EnvironmentConditions.FinalHumidity,
                InitialPressure    = dto.EnvironmentConditions.InitialPressure,
                MiddlePressure     = dto.EnvironmentConditions.MiddlePressure,
                FinalPressure      = dto.EnvironmentConditions.FinalPressure
            },
            WorkOrder             = dto.WorkOrder is null ? null : new WorkOrder
            {
                Number      = dto.WorkOrder.Number,
                IssuedAt    = dto.WorkOrder.IssuedAt,
                Description = dto.WorkOrder.Description
            }
        };

        await _assessments.AddAsync(assessment);
        await _assessments.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = assessment.Id }, MapToDto(assessment));
    }

    // -------------------------------------------------------------------------
    // PUT /api/assessments/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    [ProducesResponseType(typeof(AssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentDto>> Update(Guid id, [FromBody] AssessmentUpdateDto dto)
    {
        var labId = GetLabId();
        if (labId is null) return NoLabResult();

        var assessment = await _assessments.GetByIdAsync(id);
        if (assessment is null)
            return NotFound(new { message = $"Assessment '{id}' not found." });

        if (assessment.LabId != labId.Value)
            return Forbid();

        if (dto.TechnicianId      is not null) assessment.TechnicianId      = dto.TechnicianId.Value;
        if (dto.SupervisorId      is not null) assessment.SupervisorId      = dto.SupervisorId;
        if (dto.Status            is not null) assessment.Status            = dto.Status.Value;
        if (dto.ExpandedUncertainty is not null) assessment.ExpandedUncertainty = dto.ExpandedUncertainty.Value;
        if (dto.CoverageFactor    is not null) assessment.CoverageFactor    = dto.CoverageFactor.Value;
        if (dto.ConformityResult  is not null) assessment.ConformityResult  = dto.ConformityResult;
        if (dto.ApplicableStandard is not null) assessment.ApplicableStandard = dto.ApplicableStandard;
        if (dto.Language          is not null) assessment.Language          = dto.Language;
        if (dto.RejectionComment  is not null) assessment.RejectionComment  = dto.RejectionComment;
        if (dto.ApprovedAt        is not null) assessment.ApprovedAt        = dto.ApprovedAt;
        if (dto.StartDate         is not null) assessment.StartDate         = dto.StartDate;
        if (dto.CompletionDate    is not null) assessment.CompletionDate    = dto.CompletionDate;

        if (dto.Customer is not null)
            assessment.Customer = new Customer
            {
                Name          = dto.Customer.Name,
                ContactPerson = dto.Customer.ContactPerson,
                Email         = dto.Customer.Email,
                Phone         = dto.Customer.Phone
            };

        if (dto.Requestor is not null)
            assessment.Requestor = new Requestor
            {
                Name       = dto.Requestor.Name,
                Department = dto.Requestor.Department,
                Role       = dto.Requestor.Role,
                Email      = dto.Requestor.Email
            };

        if (dto.EnvironmentConditions is not null)
            assessment.EnvironmentConditions = new EnvironmentConditions
            {
                InitialTemperature = dto.EnvironmentConditions.InitialTemperature,
                MiddleTemperature  = dto.EnvironmentConditions.MiddleTemperature,
                FinalTemperature   = dto.EnvironmentConditions.FinalTemperature,
                InitialHumidity    = dto.EnvironmentConditions.InitialHumidity,
                MiddleHumidity     = dto.EnvironmentConditions.MiddleHumidity,
                FinalHumidity      = dto.EnvironmentConditions.FinalHumidity,
                InitialPressure    = dto.EnvironmentConditions.InitialPressure,
                MiddlePressure     = dto.EnvironmentConditions.MiddlePressure,
                FinalPressure      = dto.EnvironmentConditions.FinalPressure
            };

        if (dto.WorkOrder is not null)
            assessment.WorkOrder = new WorkOrder
            {
                Number      = dto.WorkOrder.Number,
                IssuedAt    = dto.WorkOrder.IssuedAt,
                Description = dto.WorkOrder.Description
            };

        _assessments.Update(assessment);
        await _assessments.SaveChangesAsync();

        return Ok(MapToDto(assessment));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/assessments/{id}
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

        var assessment = await _assessments.GetByIdAsync(id);
        if (assessment is null)
            return NotFound(new { message = $"Assessment '{id}' not found." });

        if (assessment.LabId != labId.Value)
            return Forbid();

        _assessments.Delete(assessment);
        await _assessments.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static AssessmentDto MapToDto(Assessment a) => new()
    {
        Id                    = a.Id,
        LabId                 = a.LabId,
        ItemId                = a.ItemId,
        ReferenceStandardId   = a.ReferenceStandardId,
        StandardCertificateId = a.StandardCertificateId,
        MethodId              = a.MethodId,
        TechnicianId          = a.TechnicianId,
        SupervisorId          = a.SupervisorId,
        Status                = a.Status,
        ExpandedUncertainty   = a.ExpandedUncertainty,
        CoverageFactor        = a.CoverageFactor,
        ConformityResult      = a.ConformityResult,
        ApplicableStandard    = a.ApplicableStandard,
        Language              = a.Language,
        RejectionComment      = a.RejectionComment,
        PerformedAt           = a.PerformedAt,
        ApprovedAt            = a.ApprovedAt,
        StartDate             = a.StartDate,
        CompletionDate        = a.CompletionDate,
        Customer              = a.Customer is null ? null : new AssessmentCustomerDto
        {
            Name          = a.Customer.Name,
            ContactPerson = a.Customer.ContactPerson,
            Email         = a.Customer.Email,
            Phone         = a.Customer.Phone
        },
        Requestor             = a.Requestor is null ? null : new AssessmentRequestorDto
        {
            Name       = a.Requestor.Name,
            Department = a.Requestor.Department,
            Role       = a.Requestor.Role,
            Email      = a.Requestor.Email
        },
        EnvironmentConditions = a.EnvironmentConditions is null ? null : new AssessmentEnvironmentDto
        {
            InitialTemperature = a.EnvironmentConditions.InitialTemperature,
            MiddleTemperature  = a.EnvironmentConditions.MiddleTemperature,
            FinalTemperature   = a.EnvironmentConditions.FinalTemperature,
            InitialHumidity    = a.EnvironmentConditions.InitialHumidity,
            MiddleHumidity     = a.EnvironmentConditions.MiddleHumidity,
            FinalHumidity      = a.EnvironmentConditions.FinalHumidity,
            InitialPressure    = a.EnvironmentConditions.InitialPressure,
            MiddlePressure     = a.EnvironmentConditions.MiddlePressure,
            FinalPressure      = a.EnvironmentConditions.FinalPressure
        },
        WorkOrder             = a.WorkOrder is null ? null : new AssessmentWorkOrderDto
        {
            Number      = a.WorkOrder.Number,
            IssuedAt    = a.WorkOrder.IssuedAt,
            Description = a.WorkOrder.Description
        },
        CreatedAt  = a.CreatedAt,
        UpdatedAt  = a.UpdatedAt
    };
}
