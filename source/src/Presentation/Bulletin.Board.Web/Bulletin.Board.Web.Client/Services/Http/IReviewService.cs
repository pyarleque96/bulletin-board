using Bulletin.Board.Web.Client.Models;
using Bulletin.Board.Web.Client.Services.Common;

namespace Bulletin.Board.Web.Client.Services.Http;

/// <summary>
/// Read-only review service for the /manage panel.
/// Providers can read GM-approved reviews for their listings.
/// Rating submission is out of scope for /manage (public endpoint only).
/// </summary>
public interface IReviewService
{
    /// <summary>Paginated list of GM-approved reviews for an owned listing.</summary>
    Task<ApiResult<PagedResultDto<ListingReviewDto>>> GetByListingAsync(
        Guid              listingId,
        int               page     = 1,
        int               pageSize = 20,
        CancellationToken ct       = default);
}
