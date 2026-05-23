using Asp.Versioning;
using Bulletin.Board.Application.Commands.Contacts;
using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Listings;
using Bulletin.Board.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/listings")]
public class ListingsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns the top N approved listings for the home page, ordered by the canonical
    /// ranking: ProviderHierarchy DESC, ProviderTier DESC, AvgRating DESC, CreatedAt DESC, Id DESC.
    /// Default count is 10. Max 50.
    /// </summary>
    [HttpGet("top")]
    [ProducesResponseType(typeof(IReadOnlyList<ListingCardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ListingCardDto>>> GetTop(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetTopListingsQuery(count), ct);
        return Ok(result);
    }

    /// <summary>Searches listings with optional filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ListingCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PagedResult<ListingCardDto>>> Search(
        [FromQuery] string? category,
        [FromQuery] string? tier,
        [FromQuery] decimal? minRating,
        [FromQuery] string? language,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new SearchListingsQuery(category, tier, minRating, language, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Returns the full detail of a listing by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ListingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingDetailDto>> GetDetail(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetListingDetailQuery(id), ct);
        if (result is null)
            return Problem(
                title: "Listing not found",
                detail: $"Listing with ID '{id}' does not exist or is not publicly available.",
                statusCode: StatusCodes.Status404NotFound);

        return Ok(result);
    }

    /// <summary>
    /// Returns the full detail of a specific listing owned by the current user.
    /// Works regardless of listing status (Pending, NeedsChanges, Rejected, Approved).
    /// Returns 404 when the listing does not exist or belongs to a different user —
    /// existence is never disclosed to prevent enumeration.
    /// </summary>
    [HttpGet("/api/v{version:apiVersion}/me/listings/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ListingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingDetailDto>> GetMyListingDetail(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyListingDetailQuery(id), ct);
        if (result is null)
            return Problem(
                title: "Listing not found",
                detail: $"Listing with ID '{id}' does not exist or is not accessible.",
                statusCode: StatusCodes.Status404NotFound);

        return Ok(result);
    }

    /// <summary>Returns the listings owned by the current user, paginated. Owner panel.</summary>
    [HttpGet("/api/v{version:apiVersion}/me/listings")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<MyListingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<MyListingDto>>> GetMyListings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetMyListingsQuery(page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Returns the full detail of a listing by its public slug.</summary>
    [HttpGet("by-slug/{slug:minlength(1):maxlength(120)}")]
    [ProducesResponseType(typeof(ListingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingDetailDto>> GetDetailBySlug(string slug, CancellationToken ct)
    {
        var result = await mediator.Send(new GetListingDetailBySlugQuery(slug), ct);
        if (result is null)
            return Problem(
                title: "Listing not found",
                detail: $"Listing with slug '{slug}' does not exist or is not publicly available.",
                statusCode: StatusCodes.Status404NotFound);

        return Ok(result);
    }

    /// <summary>
    /// Records an anonymous view event for a listing. Powers the analytics tab and the
    /// <c>views_30d</c> term of the trending score. No auth required.
    /// Caller MUST supply a stable per-session hash; the server throttles repeated views
    /// from the same (listingId, sessionHash) for 5 minutes. Returns 204 always — the
    /// endpoint never leaks whether the throttle hit or the listing exists.
    /// </summary>
    [HttpPost("{id:guid}/view")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RecordView(
        Guid id,
        [FromBody] RecordViewRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new RecordListingViewCommand(id, request.SessionHash, null), ct);
        return NoContent();
    }

    /// <summary>
    /// Records a contact interaction for a listing after the user accepts the legal waiver.
    /// Returns a WhatsApp URL to initiate conversation with the provider.
    /// </summary>
    [HttpPost("{id:guid}/contact")]
    [ProducesResponseType(typeof(ContactResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResultDto>> Contact(
        Guid id,
        [FromBody] ContactRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ContactProviderCommand(id, request.WaiverAccepted), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ─── Owner panel endpoints ────────────────────────────────────────────────

    /// <summary>
    /// Edits a listing's basic info. Owner-only.
    /// Regla #5: any provider edit reverts the listing to Pending status.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ListingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingDetailDto>> UpdateListing(
        Guid id,
        [FromBody] UpdateListingRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateOwnListingCommand(
                id,
                request.TitleEn,
                request.TitleEs,
                request.DescriptionEn,
                request.DescriptionEs,
                request.Price,
                request.Location,
                request.WhatsAppNumber),
            ct);
        return Ok(result);
    }

    /// <summary>
    /// Pauses or resumes a listing. Owner-only. Does NOT trigger re-approval.
    /// Paused listings disappear from public search but remain in the owner dashboard.
    /// </summary>
    [HttpPatch("{id:guid}/active")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetActive(
        Guid id,
        [FromBody] SetListingActiveRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new SetListingActiveCommand(id, request.IsActive), ct);
        return NoContent();
    }

    /// <summary>
    /// Soft-deletes a listing. Owner-only. Cannot be undone by the owner (Admin can restore).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteOwn(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteOwnListingCommand(id), ct);
        return NoContent();
    }

    /// <summary>
    /// Returns KPI analytics for a listing. Owner-only.
    /// Views-related metrics return 0 until listing_views tracking is implemented.
    /// </summary>
    [HttpGet("{id:guid}/analytics")]
    [Authorize]
    [ProducesResponseType(typeof(ListingAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingAnalyticsDto>> GetAnalytics(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetListingAnalyticsQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns a paginated list of approved reviews for a listing. Owner-only.
    /// </summary>
    [HttpGet("{id:guid}/reviews")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<ListingReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ListingReviewDto>>> GetReviews(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetListingReviewsQuery(id, page, pageSize), ct);
        return Ok(result);
    }
}

public record ContactRequest(bool WaiverAccepted);

public record RecordViewRequest(string SessionHash);

public record UpdateListingRequest(
    string TitleEn,
    string TitleEs,
    string? DescriptionEn,
    string? DescriptionEs,
    decimal? Price,
    string? Location,
    string? WhatsAppNumber);

public record SetListingActiveRequest(bool IsActive);
