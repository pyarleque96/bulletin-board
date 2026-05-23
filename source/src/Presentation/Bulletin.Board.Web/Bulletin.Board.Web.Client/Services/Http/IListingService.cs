using Bulletin.Board.Web.Client.Models;
using Bulletin.Board.Web.Client.Services.Common;

namespace Bulletin.Board.Web.Client.Services.Http;

/// <summary>
/// Owner-facing listing service for the /manage panel.
/// All methods return <see cref="ApiResult{T}"/> — never null, never throw on HTTP errors.
/// </summary>
public interface IListingService
{
    /// <summary>Paginated list of the authenticated user's listings with optional filters.</summary>
    Task<ApiResult<PagedResultDto<MyListingDto>>> GetMyListingsAsync(
        int    page       = 1,
        int    pageSize   = 20,
        string? status    = null,
        string? category  = null,
        string? search    = null,
        CancellationToken ct = default);

    /// <summary>
    /// Full detail of a single listing.
    /// Currently maps to GET /api/v1/listings/{id} (ListingDetailDto).
    /// After B2 lands, this will point to an owner-specific endpoint with editable fields.
    /// </summary>
    Task<ApiResult<ListingDetailDto>> GetListingByIdAsync(
        Guid listingId,
        CancellationToken ct = default);

    /// <summary>KPI analytics for a single owned listing.</summary>
    Task<ApiResult<ListingAnalyticsDto>> GetAnalyticsAsync(
        Guid listingId,
        CancellationToken ct = default);

    /// <summary>
    /// Updates an owned listing. Per Regla #5 the backend forces Status = Pending on every edit.
    /// </summary>
    Task<ApiResult<bool>> UpdateAsync(
        Guid                 listingId,
        UpdateListingRequest request,
        CancellationToken    ct = default);

    /// <summary>Pauses or re-activates a listing (PATCH /listings/{id}/active).</summary>
    Task<ApiResult<bool>> SetActiveAsync(
        Guid              listingId,
        bool              isActive,
        CancellationToken ct = default);

    /// <summary>Permanently deletes an owned listing.</summary>
    Task<ApiResult<bool>> DeleteAsync(
        Guid              listingId,
        CancellationToken ct = default);
}
