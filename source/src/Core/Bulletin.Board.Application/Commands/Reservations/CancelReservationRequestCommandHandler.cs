using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Reservations;

public sealed class CancelReservationRequestCommandHandler(
    IReservationRequestRepository reservations,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser,
    ILogger<CancelReservationRequestCommandHandler> logger)
    : IRequestHandler<CancelReservationRequestCommand>
{
    public async Task Handle(CancelReservationRequestCommand request, CancellationToken ct)
    {
        var reservation = await reservations.GetWithRefsAsync(request.ReservationId, ct)
            ?? throw new InvalidOperationException("Reservation not found.");

        ReservationAuthorization.AuthorizeCancel(reservation, currentUser);

        var wasAccepted = reservation.Status == ReservationStatus.Accepted;
        reservation.Cancel(request.Note);

        // If the reservation was Accepted, an associated Reserved blackout exists — remove it
        // so the dates free up for new reservations.
        if (wasAccepted)
        {
            var blackout = await reservations.FindBlackoutForReservationAsync(reservation.Id, ct);
            if (blackout is not null)
                vehicles.RemoveUnavailability(blackout);
        }

        await reservations.SaveChangesAsync(ct);

        logger.LogInformation(
            "Reservation {Id} cancelled by {UserId} (wasAccepted={WasAccepted}).",
            reservation.Id, currentUser.UserId, wasAccepted);
    }
}
