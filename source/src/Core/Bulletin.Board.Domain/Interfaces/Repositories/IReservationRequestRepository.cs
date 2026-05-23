using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;

namespace Bulletin.Board.Domain.Interfaces.Repositories;

public interface IReservationRequestRepository : IRepository<ReservationRequest>
{
    /// <summary>Loads a reservation with Vehicle + Listing navigations for mutation flows.</summary>
    Task<ReservationRequest?> GetWithRefsAsync(Guid id, CancellationToken ct = default);

    /// <summary>All reservations for a listing, optionally filtered by status. Owner panel.</summary>
    Task<IReadOnlyList<ReservationRequest>> GetForListingAsync(
        Guid listingId, ReservationStatus? status, CancellationToken ct = default);

    /// <summary>
    /// Paginates reservations for a listing, optionally filtered by status. Orden estable: CreatedAt DESC, Id ASC.
    /// Returns (page items, total count for current filter, has-any flag ignoring filter).
    /// </summary>
    Task<(IReadOnlyList<ReservationRequest> Items, int Total, bool HasAnyUnfiltered)>
        GetForListingPagedAsync(
            Guid listingId, ReservationStatus? status, int skip, int take, CancellationToken ct = default);

    /// <summary>All reservations a user has submitted, newest first. Requester history.</summary>
    Task<IReadOnlyList<ReservationRequest>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Finds the blackout that an Accepted reservation produced, for cascade-on-cancel.</summary>
    Task<VehicleUnavailability?> FindBlackoutForReservationAsync(Guid reservationId, CancellationToken ct = default);
}
