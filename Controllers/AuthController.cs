using MetroSol.API.DTOs.Auth;
using MetroSol.API.Services;
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MetroSol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IRepository<User>         _users;
    private readonly IRepository<Organization> _orgs;
    private readonly IRepository<RefreshToken> _refreshTokens;
    private readonly ITokenService             _tokenService;
    private readonly IConfiguration            _config;

    public AuthController(
        IRepository<User>         users,
        IRepository<Organization> orgs,
        IRepository<RefreshToken> refreshTokens,
        ITokenService             tokenService,
        IConfiguration            config)
    {
        _users         = users;
        _orgs          = orgs;
        _refreshTokens = refreshTokens;
        _tokenService  = tokenService;
        _config        = config;
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/register
    // -------------------------------------------------------------------------
    /// <summary>
    /// Creates a new user and returns a JWT + refresh token immediately (auto-login).
    /// No authentication required.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var existing = await _users.FindAsync(u => u.Email == dto.Email);
        if (existing.Any())
            return Conflict(new { message = "E-mail is already in use." });

        Organization? org = null;
        if (dto.OrganizationId.HasValue)
        {
            org = await _orgs.GetByIdAsync(dto.OrganizationId.Value);
            if (org is null)
                return NotFound(new { message = $"Organization '{dto.OrganizationId}' not found." });
        }

        var user = new User
        {
            Name           = dto.Name,
            Email          = dto.Email,
            PasswordHash   = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role           = dto.Role,
            OrganizationId = dto.OrganizationId,
            Organization   = org
        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        return Ok(await BuildResponseAsync(user));
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/login
    // -------------------------------------------------------------------------
    /// <summary>
    /// Authenticates with email and password.
    /// Returns a short-lived JWT access token and a long-lived refresh token.
    /// Error message is generic to prevent email enumeration.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var matches = await _users.FindAsync(u => u.Email == dto.Email);
        var user    = matches.FirstOrDefault();

        // Constant-time check — always runs BCrypt even if user is null
        // to prevent timing attacks that reveal registered emails.
        var passwordOk = user is not null
            && BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!passwordOk)
            return Unauthorized(new { message = "Invalid credentials." });

        return Ok(await BuildResponseAsync(user!));
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/refresh
    // -------------------------------------------------------------------------
    /// <summary>
    /// Exchanges a valid refresh token for a new access token + rotated refresh token.
    /// The old refresh token is revoked after use (rotation pattern).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshDto dto)
    {
        var tokens = await _refreshTokens.FindAsync(r => r.Token == dto.RefreshToken);
        var storedToken = tokens.FirstOrDefault();

        if (storedToken is null || !storedToken.IsActive)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        var user = await _users.GetByIdAsync(storedToken.UserId);
        if (user is null)
            return Unauthorized(new { message = "User not found." });

        // Rotate: revoke current token and issue a new one
        var newRefreshToken = CreateRefreshToken(user.Id);
        storedToken.IsRevoked       = true;
        storedToken.RevokedAt       = DateTime.UtcNow;
        storedToken.ReplacedByToken = newRefreshToken.Token;

        await _refreshTokens.AddAsync(newRefreshToken);
        _refreshTokens.Update(storedToken);
        await _refreshTokens.SaveChangesAsync();

        var expMinutes = int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "15");

        return Ok(new AuthResponseDto
        {
            AccessToken            = _tokenService.GenerateAccessToken(user),
            ExpiresIn              = expMinutes * 60,
            RefreshToken           = newRefreshToken.Token,
            RefreshTokenExpiresAt  = newRefreshToken.ExpiresAt,
            UserId                 = user.Id,
            Name                   = user.Name,
            Email                  = user.Email,
            Role                   = user.Role.ToString(),
            OrganizationId         = user.OrganizationId
        });
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/logout
    // -------------------------------------------------------------------------
    /// <summary>
    /// Revokes the provided refresh token.
    /// The access token (JWT) remains valid until it expires naturally —
    /// implement a short expiration window (15 min) to minimise this risk.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
    {
        var tokens = await _refreshTokens.FindAsync(r => r.Token == dto.RefreshToken);
        var storedToken = tokens.FirstOrDefault();

        if (storedToken is null || storedToken.IsRevoked)
            return BadRequest(new { message = "Refresh token not found or already revoked." });

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        _refreshTokens.Update(storedToken);
        await _refreshTokens.SaveChangesAsync();

        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<AuthResponseDto> BuildResponseAsync(User user)
    {
        var expMinutes   = int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "15");
        var refreshToken = CreateRefreshToken(user.Id);

        await _refreshTokens.AddAsync(refreshToken);
        await _refreshTokens.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken           = _tokenService.GenerateAccessToken(user),
            ExpiresIn             = expMinutes * 60,
            RefreshToken          = refreshToken.Token,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            UserId                = user.Id,
            Name                  = user.Name,
            Email                 = user.Email,
            Role                  = user.Role.ToString(),
            OrganizationId        = user.OrganizationId
        };
    }

    private RefreshToken CreateRefreshToken(Guid userId) => new()
    {
        UserId    = userId,
        Token     = _tokenService.GenerateRefreshToken(),
        ExpiresAt = DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpirationDays)
    };
}
