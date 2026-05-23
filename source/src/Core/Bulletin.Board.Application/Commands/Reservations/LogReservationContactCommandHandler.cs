using Bulletin.Board.Application.Settings;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bulletin.Board.Application.Commands.Reservations;

public sealed class LogReservationContactCommandHandler(
    IReservationRequestRepository reservations,
    IRepository<Contact> contactRepository,
    ICurrentUserService currentUser,
    IEmailService emailService,
    IOptions<NotificationSettings> notificationOptions,
    ILogger<LogReservationContactCommandHandler> logger)
    : IRequestHandler<LogReservationContactCommand>
{
    public async Task Handle(LogReservationContactCommand request, CancellationToken ct)
    {
        var reservation = await reservations.GetWithRefsAsync(request.ReservationId, ct)
            ?? throw new InvalidOperationException("Reservation not found.");

        // Any authenticated party who is part of the reservation (owner or requester) can log contact.
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        var isOwner = reservation.Listing?.Provider?.UserId == userId;
        var isRequester = reservation.RequesterUserId == userId;

        if (!isOwner && !isRequester && !currentUser.IsInRole("Admin"))
            throw new UnauthorizedAccessException(
                "Only the listing owner, the requester, or an Admin can log contact for this reservation.");

        var listing = reservation.Listing
            ?? throw new InvalidOperationException("Reservation is missing listing reference.");

        // Reuse the existing Contact entity to track the interaction.
        var contact = Contact.Create(
            listingId: listing.Id,
            initiatorUserId: userId,
            providerId: listing.ProviderId);

        await contactRepository.AddAsync(contact, ct);
        await contactRepository.SaveChangesAsync(ct);

        // Regla #8: GM always receives an email for every contact interaction.
        var gmEmail = notificationOptions.Value.GmEmail;
        try
        {
            await emailService.SendAsync(
                to: gmEmail,
                subject: $"[Bulletin Dells] WhatsApp contact logged — reservation {reservation.Id}",
                htmlBody: $"""
                    <h2>Reservation Contact Logged</h2>
                    <p><strong>Listing:</strong> {listing.TitleEn}</p>
                    <p><strong>Reservation ID:</strong> {reservation.Id}</p>
                    <p><strong>Channel:</strong> {request.Channel}</p>
                    <p><strong>Initiated by user ID:</strong> {userId}</p>
                    <p><strong>Time:</strong> {DateTimeOffset.UtcNow:u}</p>
                    <p>Dates: {reservation.StartDate:yyyy-MM-dd} → {reservation.EndDate:yyyy-MM-dd}</p>
                    """,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to notify GM of contact log for reservation {ReservationId}.", reservation.Id);
        }

        logger.LogInformation(
            "Contact logged for reservation {ReservationId} via {Channel} by user {UserId}.",
            reservation.Id, request.Channel, userId);
    }
}
