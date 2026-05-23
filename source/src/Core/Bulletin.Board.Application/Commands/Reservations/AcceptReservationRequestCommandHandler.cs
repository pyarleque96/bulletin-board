using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Reservations;

public sealed class AcceptReservationRequestCommandHandler(
    IReservationRequestRepository reservations,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser,
    IEmailService emailService,
    ILogger<AcceptReservationRequestCommandHandler> logger)
    : IRequestHandler<AcceptReservationRequestCommand>
{
    public async Task Handle(AcceptReservationRequestCommand request, CancellationToken ct)
    {
        var reservation = await reservations.GetWithRefsAsync(request.ReservationId, ct)
            ?? throw new InvalidOperationException("Reservation not found.");

        ReservationAuthorization.AuthorizeOwnerAction(reservation, currentUser);

        // Mark accepted first (validates state). Then create the blackout — if the EXCLUDE
        // constraint blocks it (someone else booked overlapping dates between Pending submit and
        // Accept), the domain exception bubbles up and the reservation save is rolled back.
        reservation.Accept(request.Note);

        var blackout = VehicleUnavailability.Create(
            vehicleId: reservation.VehicleId,
            startDate: reservation.StartDate,
            endDate: reservation.EndDate,
            reason: UnavailabilityReason.Reserved,
            reservationId: reservation.Id);

        // AddUnavailabilityAsync persists immediately and translates EXCLUDE violations.
        await vehicles.AddUnavailabilityAsync(blackout, ct);

        // Now persist the status change (separate save — first save already locked dates).
        await reservations.SaveChangesAsync(ct);

        await TryNotifyRequesterAsync(reservation, "accepted", request.Note, ct);

        logger.LogInformation(
            "Reservation {Id} accepted by {UserId}; blackout {BlackoutId} created.",
            reservation.Id, currentUser.UserId, blackout.Id);
    }

    private async Task TryNotifyRequesterAsync(
        ReservationRequest reservation, string verb, string? note, CancellationToken ct)
    {
        try
        {
            await emailService.SendAsync(
                to: reservation.RequesterEmail,
                subject: $"[Bulletin Dells] Your reservation was {verb}",
                htmlBody: $"""
                    <h2>Reservation {verb}</h2>
                    <p>Your reservation request for {reservation.StartDate:yyyy-MM-dd} to {reservation.EndDate:yyyy-MM-dd}
                       has been <strong>{verb}</strong>.</p>
                    {(string.IsNullOrWhiteSpace(note) ? "" : $"<p><strong>Note from provider:</strong> {note}</p>")}
                    <p>Reservation ID: <code>{reservation.Id}</code></p>
                    """,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to notify requester for reservation {ReservationId} ({Verb})",
                reservation.Id, verb);
        }
    }
}
