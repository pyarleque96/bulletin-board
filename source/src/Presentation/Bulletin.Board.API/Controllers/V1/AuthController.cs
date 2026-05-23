using System.Security.Claims;
using Asp.Versioning;
using Bulletin.Board.Application.Commands.Auth;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(IMediator mediator, IWebHostEnvironment env) : ControllerBase
{
    private const string RefreshTokenCookieName = "refresh_token";

    /// <summary>Registra un nuevo usuario con rol User o Provider.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserRegisteredDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UserRegisteredDto>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterUserCommand(request.Email, request.Password, request.Role), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Autentica al usuario y devuelve un access token JWT.
    /// El refresh token se envía en una cookie HttpOnly.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccessTokenResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var deviceInfo = GetDeviceInfo();
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password, deviceInfo), ct);
        SetRefreshTokenCookie(result.RefreshTokenRaw);
        return Ok(new AccessTokenResponse(result.AccessToken, result.ExpiresInSeconds));
    }

    /// <summary>
    /// Rota el refresh token y devuelve un nuevo access token JWT.
    /// Lee el refresh token desde la cookie HttpOnly.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccessTokenResponse>> Refresh(CancellationToken ct)
    {
        var refreshToken = HttpContext.Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Problem(
                title: "Refresh token missing",
                detail: null,
                statusCode: StatusCodes.Status401Unauthorized);

        try
        {
            var result = await mediator.Send(new RefreshTokenCommand(refreshToken), ct);
            SetRefreshTokenCookie(result.RefreshTokenRaw);
            return Ok(new AccessTokenResponse(result.AccessToken, result.ExpiresInSeconds));
        }
        catch (SessionCompromisedException ex)
        {
            ClearRefreshTokenCookie();
            return Problem(
                title: "Session compromised",
                detail: null,
                statusCode: StatusCodes.Status401Unauthorized,
                extensions: new Dictionary<string, object?> { ["code"] = ex.ErrorCode });
        }
        catch (TokenExpiredException ex)
        {
            ClearRefreshTokenCookie();
            return Problem(
                title: "Token expired",
                detail: null,
                statusCode: StatusCodes.Status401Unauthorized,
                extensions: new Dictionary<string, object?> { ["code"] = ex.ErrorCode });
        }
        catch (UnauthorizedAccessException)
        {
            ClearRefreshTokenCookie();
            return Problem(
                title: "Invalid or missing refresh token",
                detail: null,
                statusCode: StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Revoca el refresh token actual del dispositivo y borra la cookie.
    /// Es idempotente: si el token ya fue revocado o no existe, responde 204.
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = HttpContext.Request.Cookies[RefreshTokenCookieName];
        await mediator.Send(new LogoutCommand(refreshToken), ct);
        ClearRefreshTokenCookie();
        return NoContent();
    }

    /// <summary>
    /// Revoca todos los refresh tokens activos del usuario autenticado (cierre global de sesión).
    /// Requiere un access token válido en el header Authorization.
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll(CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await mediator.Send(new LogoutAllCommand(userId), ct);
        ClearRefreshTokenCookie();
        return NoContent();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private string? GetDeviceInfo()
    {
        var ua = HttpContext.Request.Headers.UserAgent.FirstOrDefault();
        return ua is null ? null : ua[..Math.Min(ua.Length, 512)];
    }

    // ── Cookie helpers ──────────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string token)
    {
        // SameSite=None (con Secure=true) es necesario en desarrollo porque el frontend Blazor
        // (localhost:7031) y la API (localhost:7207) son orígenes distintos.
        // En producción, cuando comparten el mismo eTLD+1, SameSite=Strict ofrece la mejor protección CSRF.
        var sameSite = env.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict;

        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = sameSite,
            // Path restringido para que la cookie NO viaje en requests ajenos a auth.
            Path = "/api/v1/auth",
            // Max-Age = 30 días en segundos (2592000)
            MaxAge = TimeSpan.FromDays(30)
        };

        HttpContext.Response.Cookies.Append(RefreshTokenCookieName, token, options);
    }

    private void ClearRefreshTokenCookie()
    {
        var sameSite = env.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict;

        HttpContext.Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = sameSite,
            Path = "/api/v1/auth"
        });
    }
}

public record RegisterRequest(string Email, string Password, string Role);
public record LoginRequest(string Email, string Password);
public record AccessTokenResponse(string AccessToken, int ExpiresIn);
