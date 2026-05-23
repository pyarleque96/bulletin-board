using Bulletin.Board.Application.Settings;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bulletin.Board.Application.Commands.Reservations;

public sealed class CreateReservationRequestCommandHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    IReservationRequestRepository reservations,
    ICurrentUserService currentUser,
    IEmailService emailService,
    IOptions<NotificationSettings> notificationOptions,
    ILogger<CreateReservationRequestCommandHandler> logger)
    : IRequestHandler<CreateReservationRequestCommand, Guid>
{
    public async Task<Guid> Handle(CreateReservationRequestCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to submit a reservation.");

        var vehicle = await vehicles.GetByIdAsync(request.VehicleId, ct)
            ?? throw new InvalidOperationException("Vehicle not found.");
        if (!vehicle.IsActive)
            throw new InvalidOperationException("This vehicle is not currently accepting reservations.");

        var listing = await listings.GetByIdAsync(vehicle.ListingId, ct)
            ?? throw new InvalidOperationException("Listing not found.");
        if (listing.IsDeleted || listing.Status != ListingStatus.Approved)
            throw new InvalidOperationException("Listing is not currently available.");

        // Advisory check — gives a friendly error before submitting. The provider may still
        // accept later, but if there's an existing blackout the request is unlikely to clear.
        // Hard enforcement happens at Accept time via the EXCLUDE constraint.
        var conflicts = await vehicles.GetUnavailabilityInRangeAsync(
            vehicle.Id, request.StartDate, request.EndDate, ct);
        if (conflicts.Count > 0)
            throw new InvalidOperationException(
                "Selected dates conflict with existing unavailability. Pick a different range.");

        var reservation = ReservationRequest.Create(
            vehicleId: vehicle.Id,
            listingId: listing.Id,
            requesterUserId: userId,
            requesterName: request.RequesterName,
            requesterEmail: request.RequesterEmail,
            requesterPhone: request.RequesterPhone,
            startDate: request.StartDate,
            endDate: request.EndDate,
            comment: request.Comment);

        await reservations.AddAsync(reservation, ct);
        await reservations.SaveChangesAsync(ct);

        // Notify provider + GM. Failures are logged but don't break the user flow.
        var providerEmail = listing.Provider?.WhatsAppNumber; // contact metadata
        var gmEmail = notificationOptions.Value.GmEmail;
        await TrySendAsync(gmEmail, vehicle, listing, reservation, ct);

        logger.LogInformation(
            "Reservation {Id} created by user {UserId} for vehicle {VehicleId} ({Start} to {End}).",
            reservation.Id, userId, vehicle.Id, request.StartDate, request.EndDate);

        return reservation.Id;
    }

    private async Task TrySendAsync(
        string gmEmail, Vehicle vehicle, Listing listing,
        ReservationRequest reservation, CancellationToken ct)
    {
        var subject = $"[Bulletin Dells] New reservation request — {listing.TitleEn} ({vehicle.Name})";
        var body = $"""
            <h2>New reservation request</h2>
            <p><strong>Listing:</strong> {listing.TitleEn}</p>
            <p><strong>Vehicle:</strong> {vehicle.Name}</p>
            <p><strong>Dates:</strong> {reservation.StartDate:yyyy-MM-dd} to {reservation.EndDate:yyyy-MM-dd}</p>
            <p><strong>Requester:</strong> {reservation.RequesterName} &lt;{reservation.RequesterEmail}&gt;
               {(string.IsNullOrWhiteSpace(reservation.RequesterPhone) ? "" : $"<br/>Phone: {reservation.RequesterPhone}")}</p>
            <p><strong>Comment:</strong> {(string.IsNullOrWhiteSpace(reservation.Comment) ? "<em>(none)</em>" : reservation.Comment)}</p>
            <p>Reservation ID: <code>{reservation.Id}</code></p>
            """;

        try
        {
            await emailService.SendAsync(to: gmEmail, subject: subject, htmlBody: body, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to send reservation notification email for reservation {ReservationId}",
                reservation.Id);
        }
    }
}
