using Asp.Versioning;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Application.Queries.Admin.Stats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/stats")]
[Authorize(Roles = "Admin")]
public class AdminStatsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminStatsDto>> GetStats(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminStatsQuery(), ct);
        return Ok(result);
    }
}
