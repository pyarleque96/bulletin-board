using Asp.Versioning;
using Bulletin.Board.Application.Commands.Admin.Vip;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Application.Queries.Admin.Audit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = "Admin")]
public class AdminVipController(IMediator mediator) : ControllerBase
{
    private const string ReauthHeader = "X-Reauth-Token";

    [HttpPost("providers/{id:guid}/vip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GrantProviderVip(Guid id, [FromBody] GrantVipRequest request, CancellationToken ct)
    {
        var token = ReadReauth();
        if (token is null) return Unauthorized();

        await mediator.Send(new GrantProviderVipCommand(
            id, request.Until, request.Indefinite, request.Reason, token, GetIp()), ct);
        return NoContent();
    }

    [HttpDelete("providers/{id:guid}/vip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RevokeProviderVip(Guid id, [FromBody] RevokeVipRequest? request, CancellationToken ct)
    {
        var token = ReadReauth();
        if (token is null) return Unauthorized();

        await mediator.Send(new RevokeProviderVipCommand(id, request?.Reason, token, GetIp()), ct);
        return NoContent();
    }

    [HttpPost("listings/{id:guid}/vip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GrantListingVip(Guid id, [FromBody] GrantVipRequest request, CancellationToken ct)
    {
        var token = ReadReauth();
        if (token is null) return Unauthorized();

        await mediator.Send(new GrantListingVipCommand(
            id, request.Until, request.Indefinite, request.Reason, token, GetIp()), ct);
        return NoContent();
    }

    [HttpDelete("listings/{id:guid}/vip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RevokeListingVip(Guid id, [FromBody] RevokeVipRequest? request, CancellationToken ct)
    {
        var token = ReadReauth();
        if (token is null) return Unauthorized();

        await mediator.Send(new RevokeListingVipCommand(id, request?.Reason, token, GetIp()), ct);
        return NoContent();
    }

    [HttpGet("vip-logs")]
    [ProducesResponseType(typeof(IReadOnlyList<VipChangeLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VipChangeLogDto>>> GetLogs(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var rows = await mediator.Send(new GetVipChangeLogsQuery(entityType, entityId, page, pageSize), ct);
        return Ok(rows);
    }

    private string? ReadReauth()
    {
        if (!Request.Headers.TryGetValue(ReauthHeader, out var values)) return null;
        var v = values.ToString();
        return string.IsNullOrWhiteSpace(v) ? null : v;
    }

    private string? GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
}

public record GrantVipRequest(DateTimeOffset? Until, bool Indefinite, string? Reason);
public record RevokeVipRequest(string? Reason);
