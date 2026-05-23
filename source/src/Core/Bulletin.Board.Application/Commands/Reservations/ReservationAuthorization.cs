using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Services;

namespace Bulletin.Board.Application.Commands.Reservations;

internal static class ReservationAuthorization
{
    private const string AdminRole = "Admin";

    /// <summary>
    /// Authorizes mutations on a reservation that only the listing owner (or Admin) may perform:
    /// Accept, Reject. Returns the same reservation for fluent chaining.
    /// </summary>
    public static ReservationRequest AuthorizeOwnerAction(
        ReservationRequest reservation, ICurrentUserService currentUser)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        if (currentUser.IsInRole(AdminRole)) return reservation;

        if (reservation.Listing?.Provider is null || reservation.Listing.Provider.UserId != userId)
            throw new UnauthorizedAccessException(
                "Only the listing owner can accept or reject this reservation.");

        return reservation;
    }

    /// <summary>
    /// Cancellation may be performed by either the listing owner OR the requester.
    /// </summary>
    public static ReservationRequest AuthorizeCancel(
        ReservationRequest reservation, ICurrentUserService currentUser)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        if (currentUser.IsInRole(AdminRole)) return reservation;

        var isRequester = reservation.RequesterUserId == userId;
        var isOwner = reservation.Listing?.Provider?.UserId == userId;

        if (!isRequester && !isOwner)
            throw new UnauthorizedAccessException(
                "Only the requester or the listing owner can cancel this reservation.");

        return reservation;
    }
}
