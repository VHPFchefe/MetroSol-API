using MetroSol.API.DTOs.User;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class UserController : ControllerBase
{
    private readonly IRepository<User>         _users;
    private readonly IRepository<Organization> _orgs;
    private readonly IRepository<Lab>          _labs;

    public UserController(
        IRepository<User>         users,
        IRepository<Organization> orgs,
        IRepository<Lab>          labs)
    {
        _users = users;
        _orgs  = orgs;
        _labs  = labs;
    }

    // ── Claim helpers ─────────────────────────────────────────────────────────

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private Guid? GetLabId()
    {
        var claim = User.FindFirstValue("lab");
        return string.IsNullOrEmpty(claim) ? null : Guid.Parse(claim);
    }

    private bool IsAdmin() => User.IsInRole("Admin");

    // -------------------------------------------------------------------------
    // GET /api/users
    // -------------------------------------------------------------------------
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        IEnumerable<User> users;

        if (IsAdmin())
            users = await _users.GetAllAsync();
        else
        {
            var labId = GetLabId();
            if (labId is null)
                return Ok(Enumerable.Empty<UserDto>());

            users = await _users.FindAsync(u => u.LabId == labId.Value);
        }

        return Ok(users.Select(MapToDto));
    }

    // -------------------------------------------------------------------------
    // GET /api/users/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _users.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { message = $"User '{id}' not found." });

        if (!IsAdmin() && user.LabId != GetLabId())
            return Forbid();

        return Ok(MapToDto(user));
    }

    // -------------------------------------------------------------------------
    // POST /api/users
    // -------------------------------------------------------------------------
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateDto dto)
    {
        var existing = await _users.FindAsync(u => u.Email == dto.Email);
        if (existing.Any())
            return Conflict(new { message = "E-mail is already in use." });

        if (dto.OrganizationId.HasValue)
        {
            var org = await _orgs.GetByIdAsync(dto.OrganizationId.Value);
            if (org is null)
                return NotFound(new { message = $"Organization '{dto.OrganizationId}' not found." });
        }

        if (dto.LabId.HasValue)
        {
            var lab = await _labs.GetByIdAsync(dto.LabId.Value);
            if (lab is null)
                return NotFound(new { message = $"Lab '{dto.LabId}' not found." });
        }

        var user = new User
        {
            Name           = dto.Name,
            Email          = dto.Email,
            PasswordHash   = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role           = dto.Role,
            OrganizationId = dto.OrganizationId,
            LabId          = dto.LabId
        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, MapToDto(user));
    }

    // -------------------------------------------------------------------------
    // PUT /api/users/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UserUpdateDto dto)
    {
        var user = await _users.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { message = $"User '{id}' not found." });

        if (!IsAdmin() && user.LabId != GetLabId())
            return Forbid();

        if (dto.Email is not null && dto.Email != user.Email)
        {
            var conflict = await _users.FindAsync(u => u.Email == dto.Email);
            if (conflict.Any())
                return Conflict(new { message = "E-mail is already in use." });

            user.Email = dto.Email;
        }

        if (dto.Name           is not null) user.Name           = dto.Name;
        if (dto.Role           is not null) user.Role           = dto.Role.Value;
        if (dto.OrganizationId is not null) user.OrganizationId = dto.OrganizationId;
        if (dto.LabId          is not null) user.LabId          = dto.LabId;

        _users.Update(user);
        await _users.SaveChangesAsync();

        return Ok(MapToDto(user));
    }

    // -------------------------------------------------------------------------
    // DELETE /api/users/{id}
    // -------------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == GetUserId())
            return BadRequest(new { message = "You cannot delete your own account." });

        var user = await _users.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { message = $"User '{id}' not found." });

        _users.Delete(user);
        await _users.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static UserDto MapToDto(User u) => new()
    {
        Id             = u.Id,
        Name           = u.Name,
        Email          = u.Email,
        Role           = u.Role,
        Status         = u.Status,
        OrganizationId = u.OrganizationId,
        LabId          = u.LabId,
        CreatedAt      = u.CreatedAt,
        UpdatedAt      = u.UpdatedAt
    };
}
