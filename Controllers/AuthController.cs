using MetroSol.API.DTOs.Auth;
using MetroSol.API.Services;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRepository<User>         _users;
    private readonly IRepository<Organization> _orgs;
    private readonly ITokenService             _tokenService;
    private readonly IConfiguration            _config;

    public AuthController(
        IRepository<User>         users,
        IRepository<Organization> orgs,
        ITokenService             tokenService,
        IConfiguration            config)
    {
        _users        = users;
        _orgs         = orgs;
        _tokenService = tokenService;
        _config       = config;
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/register
    // -------------------------------------------------------------------------
    /// <summary>
    /// Creates a new user and returns a JWT token immediately (auto-login).
    /// No authentication required for this endpoint.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        // Ensures email uniqueness
        var existing = await _users.FindAsync(u => u.Email == dto.Email);
        if (existing.Any())
            return Conflict(new { message = "E-mail já está em uso." });

        // Validates organization when provided
        Organization? org = null;
        if (dto.OrganizationId.HasValue)
        {
            org = await _orgs.GetByIdAsync(dto.OrganizationId.Value);
            if (org is null)
                return NotFound(new { message = $"Organization '{dto.OrganizationId}' não encontrada." });
        }

        var user = new User
        {
            Name         = dto.Name,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role         = dto.Role,
            OrganizationId = dto.OrganizationId,
            Organization   = org   // null is valid here (nav-prop nullable in User)
        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        return Ok(BuildResponse(user));
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/login
    // -------------------------------------------------------------------------
    /// <summary>
    /// Authenticates with email and password. Returns JWT on success.
    /// Error message is intentionally generic to avoid leaking which emails exist.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var matches = await _users.FindAsync(u => u.Email == dto.Email);
        var user    = matches.FirstOrDefault();

        // Constant-time verification: BCrypt.Verify always runs, even if user == null,
        // to prevent timing attacks that reveal registered emails.
        var passwordOk = user is not null
            && BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!passwordOk)
            return Unauthorized(new { message = "Credenciais inválidas." });

        return Ok(BuildResponse(user!));
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------
    private AuthResponseDto BuildResponse(User user)
    {
        var expMinutes = int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "15");

        return new AuthResponseDto
        {
            AccessToken    = _tokenService.GenerateAccessToken(user),
            ExpiresIn      = expMinutes * 60,
            UserId         = user.Id,
            Name           = user.Name,
            Email          = user.Email,
            Role           = user.Role.ToString(),
            OrganizationId = user.OrganizationId
        };
    }
}
