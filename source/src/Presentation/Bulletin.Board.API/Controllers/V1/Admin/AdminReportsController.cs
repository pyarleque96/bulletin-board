using Asp.Versioning;
using Bulletin.Board.Application.Commands.Admin.Reports;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Application.Queries.Admin.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/reports")]
[Authorize(Roles = "Admin")]
public class AdminReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminReportDto>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetAdminReportsQuery(status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Review(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ReviewReportCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/dismiss")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Dismiss(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DismissReportCommand(id), ct);
        return NoContent();
    }
}
