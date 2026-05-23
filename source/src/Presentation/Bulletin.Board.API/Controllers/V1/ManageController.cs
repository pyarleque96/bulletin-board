using Asp.Versioning;
using Bulletin.Board.Application.Commands.Manage.Photos;
using Bulletin.Board.Application.Commands.Manage.Reviews;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Listings;
using Bulletin.Board.Application.Queries.Manage;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1;

/// <summary>
/// Endpoints for the provider's /manage panel (v3). Hosts the cursor-paginated
/// listings index plus per-listing tab data (Overview / Photos / Reviews / Settings / Analytics).
/// Authorization is per-endpoint: the provider can only access listings owned by their user.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/manage")]
public class ManageController(IMediator mediator, ILogger<ManageController> logger) : ControllerBase
{
    /// <summary>
    /// Cursor-paginated index of the provider's listings. Replaces the page+pageSize variant
    /// at <c>/api/v1/me/listings</c> (still available for one sprint).
    /// </summary>
    [HttpGet("listings")]
    [ProducesResponseType(typeof(CursorPagedResultDto<MyListingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CursorPagedResultDto<MyListingDto>>> GetListings(
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] int? page = null,
        CancellationToken ct = default)
    {
        ListingStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<ListingStatus>(status, ignoreCase: true, out var s))
                return Problem(
                    title: "Invalid status",
                    detail: $"Status '{status}' is not a valid listing status.",
                    statusCode: StatusCodes.Status400BadRequest);
            parsedStatus = s;
        }

        // Backwards-compat: si llega ?page=... advertimos en log y delegamos en la query
        // legacy. Frontend tiene 1 sprint para migrar.
        if (page.HasValue && string.IsNullOrEmpty(cursor))
        {
            logger.LogWarning(
                "Deprecated page+pageSize hit on /manage/listings — caller should migrate to cursor pagination");
            var legacy = await mediator.Send(
                new GetMyListingsQuery(page.Value, pageSize), ct);
            // Bridge: emit como CursorPagedResultDto sin cursor — el cliente solo iterará una vez.
            return Ok(new CursorPagedResultDto<MyListingDto>(
                legacy.Items, NextCursor: null, HasMore: legacy.HasNext, TotalItems: legacy.TotalItems));
        }

        try
        {
            var result = await mediator.Send(
                new GetManageListingsCursorQuery(
                    cursor, pageSize, parsedStatus, category, search,
                    IncludeTotal: string.IsNullOrEmpty(cursor)),
                ct);
            return Ok(result);
        }
        catch (DomainException ex) when (ex.Message == "Api_Error_InvalidCursor")
        {
            return Problem(
                title: "Invalid cursor",
                detail: "The provided cursor is malformed or expired.",
                statusCode: StatusCodes.Status400BadRequest,
                type: "Api_Error_InvalidCursor");
        }
    }

    // ─── Tab Overview ────────────────────────────────────────────────

    [HttpGet("listings/{id:guid}/overview")]
    [ProducesResponseType(typeof(ListingOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingOverviewDto>> GetOverview(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetListingOverviewQuery(id), ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // ─── Tab Photos ──────────────────────────────────────────────────

    [HttpGet("listings/{id:guid}/photos")]
    [ProducesResponseType(typeof(IReadOnlyList<PhotoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PhotoDto>>> GetPhotos(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetListingPhotosQuery(id), ct);
        return Ok(result);
    }

    [HttpPatch("listings/{id:guid}/photos/{photoId:guid}")]
    [ProducesResponseType(typeof(PhotoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoDto>> UpdatePhotoMetadata(
        Guid id, Guid photoId,
        [FromBody] UpdatePhotoMetadataRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdatePhotoMetadataCommand(id, photoId, request.AltEn, request.AltEs, request.Position), ct);
        return Ok(result);
    }

    [HttpDelete("listings/{id:guid}/photos/{photoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePhoto(Guid id, Guid photoId, CancellationToken ct)
    {
        await mediator.Send(new DeletePhotoCommand(id, photoId), ct);
        return NoContent();
    }

    // ─── Tab Reviews ─────────────────────────────────────────────────

    [HttpGet("listings/{id:guid}/reviews")]
    [ProducesResponseType(typeof(IReadOnlyList<ManageReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ManageReviewDto>>> GetReviews(
        Guid id, [FromQuery] string? status = null, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetManageReviewsQuery(id, status), ct);
        return Ok(result);
    }

    [HttpGet("listings/{id:guid}/reviews/summary")]
    [ProducesResponseType(typeof(RatingSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RatingSummaryDto>> GetReviewSummary(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetReviewSummaryQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("listings/{id:guid}/reviews/{reviewId:guid}/reply")]
    [ProducesResponseType(typeof(ReviewReplyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewReplyDto>> SubmitReply(
        Guid id, Guid reviewId,
        [FromBody] SubmitReviewReplyRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(new SubmitReviewReplyCommand(id, reviewId, request.Text), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}

public record UpdatePhotoMetadataRequest(string? AltEn, string? AltEs, int Position);
public record SubmitReviewReplyRequest(string Text);
