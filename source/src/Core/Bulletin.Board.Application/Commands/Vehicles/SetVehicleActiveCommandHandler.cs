using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Vehicles;

public sealed class SetVehicleActiveCommandHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser,
    ILogger<SetVehicleActiveCommandHandler> logger)
    : IRequestHandler<SetVehicleActiveCommand>
{
    public async Task Handle(SetVehicleActiveCommand request, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByIdAsync(request.VehicleId, ct)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var listing = await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, vehicle.ListingId, ct);

        if (request.IsActive) vehicle.Reactivate();
        else                  vehicle.Deactivate();

        listing.RequireReapproval();
        await vehicles.SaveChangesAsync(ct);

        logger.LogInformation(
            "Vehicle {VehicleId} active state set to {IsActive}; listing {ListingId} reverted to Pending.",
            vehicle.Id, request.IsActive, listing.Id);
    }
}
