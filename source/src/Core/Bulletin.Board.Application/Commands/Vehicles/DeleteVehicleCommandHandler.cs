using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Vehicles;

public sealed class DeleteVehicleCommandHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser,
    ILogger<DeleteVehicleCommandHandler> logger)
    : IRequestHandler<DeleteVehicleCommand>
{
    public async Task Handle(DeleteVehicleCommand request, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByIdAsync(request.VehicleId, ct)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var listing = await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, vehicle.ListingId, ct);

        // Hard delete — cascade in BD removes photos and unavailability rows.
        // The provider can also Deactivate to keep history; this is the destructive option.
        vehicles.Remove(vehicle);
        listing.RequireReapproval();
        await vehicles.SaveChangesAsync(ct);

        logger.LogInformation(
            "Vehicle {VehicleId} deleted from listing {ListingId}; listing reverted to Pending.",
            vehicle.Id, listing.Id);
    }
}
