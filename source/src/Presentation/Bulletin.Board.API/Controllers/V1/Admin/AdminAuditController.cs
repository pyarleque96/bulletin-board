using Asp.Versioning;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Application.Queries.Admin.Audit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/audit")]
[Authorize(Roles = "Admin")]
public class AdminAuditController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAll(
        [FromQuery] string? action,
        [FromQuery] string? resourceType,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetAuditLogsQuery(action, resourceType, from, to, page, pageSize), ct);
        return Ok(result);
    }
}
