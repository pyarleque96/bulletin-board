using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Vehicles.Availability;

public sealed class DeleteVehicleUnavailabilityCommandHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser,
    ILogger<DeleteVehicleUnavailabilityCommandHandler> logger)
    : IRequestHandler<DeleteVehicleUnavailabilityCommand>
{
    public async Task Handle(DeleteVehicleUnavailabilityCommand request, CancellationToken ct)
    {
        var blackout = await vehicles.GetUnavailabilityByIdAsync(request.UnavailabilityId, ct)
            ?? throw new InvalidOperationException("Unavailability entry not found.");

        await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, blackout.Vehicle.ListingId, ct);

        // Reserved blackouts are owned by the reservation system. Removing one would leave
        // an active reservation pointing to a vehicle marked free — refuse and direct the
        // owner to cancel/decline the underlying reservation instead.
        if (blackout.Reason == UnavailabilityReason.Reserved)
            throw new InvalidOperationException(
                "Reserved blackouts cannot be deleted directly. Cancel the reservation instead.");

        vehicles.RemoveUnavailability(blackout);
        await vehicles.SaveChangesAsync(ct);

        logger.LogInformation(
            "Blackout {Id} removed from vehicle {VehicleId}.",
            blackout.Id, blackout.VehicleId);
    }
}
