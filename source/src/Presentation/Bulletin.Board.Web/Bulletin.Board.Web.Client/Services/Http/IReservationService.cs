using Bulletin.Board.Web.Client.Models;
using Bulletin.Board.Web.Client.Services.Common;

namespace Bulletin.Board.Web.Client.Services.Http;

/// <summary>
/// Owner-facing reservation service for the /manage/reservations panel.
/// All methods return <see cref="ApiResult{T}"/> — never null, never throw on HTTP errors.
/// </summary>
public interface IReservationService
{
    /// <summary>Paginated reservations for a listing, optionally filtered by status.</summary>
    Task<ApiResult<PagedResultDto<ReservationDto>>> GetByListingAsync(
        Guid              listingId,
        string?           status   = null,
        int               page     = 1,
        int               pageSize = 20,
        CancellationToken ct       = default);

    /// <summary>Accepts a pending reservation with an optional note.</summary>
    Task<ApiResult<bool>> AcceptAsync(
        Guid              reservationId,
        string?           note = null,
        CancellationToken ct   = default);

    /// <summary>Rejects a pending reservation with an optional note.</summary>
    Task<ApiResult<bool>> RejectAsync(
        Guid              reservationId,
        string?           note = null,
        CancellationToken ct   = default);

    /// <summary>Cancels an accepted reservation with an optional note.</summary>
    Task<ApiResult<bool>> CancelAsync(
        Guid              reservationId,
        string?           note = null,
        CancellationToken ct   = default);

    /// <summary>Marks a reservation as completed (vehicle returned).</summary>
    Task<ApiResult<bool>> MarkCompletedAsync(
        Guid              reservationId,
        CancellationToken ct = default);
}
