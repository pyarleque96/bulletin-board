using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Reservations;

public sealed class RejectReservationRequestCommandHandler(
    IReservationRequestRepository reservations,
    ICurrentUserService currentUser,
    IEmailService emailService,
    ILogger<RejectReservationRequestCommandHandler> logger)
    : IRequestHandler<RejectReservationRequestCommand>
{
    public async Task Handle(RejectReservationRequestCommand request, CancellationToken ct)
    {
        var reservation = await reservations.GetWithRefsAsync(request.ReservationId, ct)
            ?? throw new InvalidOperationException("Reservation not found.");

        ReservationAuthorization.AuthorizeOwnerAction(reservation, currentUser);
        reservation.Reject(request.Note);
        await reservations.SaveChangesAsync(ct);

        await TryNotifyAsync(reservation, request.Note, ct);

        logger.LogInformation("Reservation {Id} rejected by {UserId}.", reservation.Id, currentUser.UserId);
    }

    private async Task TryNotifyAsync(ReservationRequest reservation, string? note, CancellationToken ct)
    {
        try
        {
            await emailService.SendAsync(
                to: reservation.RequesterEmail,
                subject: "[Bulletin Dells] Your reservation request was declined",
                htmlBody: $"""
                    <p>Your reservation request for {reservation.StartDate:yyyy-MM-dd} to {reservation.EndDate:yyyy-MM-dd}
                       has been <strong>declined</strong>.</p>
                    {(string.IsNullOrWhiteSpace(note) ? "" : $"<p><strong>Note:</strong> {note}</p>")}
                    """,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to notify requester for rejection of {ReservationId}", reservation.Id);
        }
    }
}
