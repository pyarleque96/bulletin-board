using Asp.Versioning;
using Bulletin.Board.Application.Commands.Admin.Ratings;
using Bulletin.Board.Application.Commands.Admin.Reviews;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Application.Queries.Admin.Ratings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/ratings")]
[Authorize(Roles = "Admin")]
public class AdminRatingsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminRatingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminRatingDto>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetAdminRatingsQuery(status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Approve(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ApproveRatingCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reject(Guid id, CancellationToken ct)
    {
        await mediator.Send(new RejectRatingCommand(id), ct);
        return NoContent();
    }

    /// <summary>
    /// Moderates a provider reply to a review (regla #7). Body: <c>{ Approve: bool, GmFeedback?: string }</c>.
    /// </summary>
    [HttpPatch("replies/{replyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ModerateReply(
        Guid replyId,
        [FromBody] ModerateReplyRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new ModerateReviewReplyCommand(replyId, request.Approve, request.GmFeedback), ct);
        return NoContent();
    }
}

public record ModerateReplyRequest(bool Approve, string? GmFeedback);
