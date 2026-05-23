using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Reservations;

public sealed class MarkReservationCompletedCommandHandler(
    IReservationRequestRepository reservations,
    ICurrentUserService currentUser,
    ILogger<MarkReservationCompletedCommandHandler> logger)
    : IRequestHandler<MarkReservationCompletedCommand>
{
    public async Task Handle(MarkReservationCompletedCommand request, CancellationToken ct)
    {
        var reservation = await reservations.GetWithRefsAsync(request.ReservationId, ct)
            ?? throw new InvalidOperationException("Reservation not found.");

        // Only the listing owner may mark as completed.
        ReservationAuthorization.AuthorizeOwnerAction(reservation, currentUser);

        // DomainException bubbles up as 400 via the global exception handler when
        // the state is anything other than Accepted.
        reservation.MarkCompleted();

        await reservations.SaveChangesAsync(ct);

        logger.LogInformation(
            "Reservation {ReservationId} marked as completed by owner {UserId}.",
            reservation.Id, currentUser.UserId);
    }
}
