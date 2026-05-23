using Asp.Versioning;
using Bulletin.Board.Application.Commands.Admin.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Application.Queries.Admin.Listings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/listings")]
[Authorize(Roles = "Admin")]
public class AdminListingsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminListingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminListingDto>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? tier,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? providerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetAdminListingsQuery(status, tier, categoryId, providerId, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminListingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminListingDetailDto>> GetDetail(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminListingDetailQuery(id), ct);
        if (result is null)
            return Problem(title: "Listing not found", statusCode: StatusCodes.Status404NotFound);

        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Approve(Guid id, [FromBody] ApproveListingRequest request, CancellationToken ct)
    {
        await mediator.Send(new ApproveListingCommand(id, request.FeedbackEn, request.FeedbackEs), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reject(Guid id, [FromBody] RejectListingRequest request, CancellationToken ct)
    {
        await mediator.Send(new RejectListingCommand(id, request.Reason), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/request-changes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RequestChanges(Guid id, [FromBody] RequestChangesRequest request, CancellationToken ct)
    {
        await mediator.Send(new RequestListingChangesCommand(id, request.Feedback), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteListingCommand(id), ct);
        return NoContent();
    }
}

public record ApproveListingRequest(string? FeedbackEn, string? FeedbackEs);
public record RejectListingRequest(string Reason);
public record RequestChangesRequest(string Feedback);
