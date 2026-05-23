using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Vehicles.Photos;

public sealed class SetVehiclePhotoPrimaryCommandHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser,
    ILogger<SetVehiclePhotoPrimaryCommandHandler> logger)
    : IRequestHandler<SetVehiclePhotoPrimaryCommand>
{
    public async Task Handle(SetVehiclePhotoPrimaryCommand request, CancellationToken ct)
    {
        var photo = await vehicles.GetPhotoByIdAsync(request.PhotoId, ct)
            ?? throw new InvalidOperationException("Photo not found.");

        var listing = await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, photo.Vehicle.ListingId, ct);

        // Demote any existing primary on this vehicle, then promote the target.
        await vehicles.UnsetPrimaryForAllPhotosAsync(photo.VehicleId, ct);
        photo.MarkPrimary();

        listing.RequireReapproval();
        await vehicles.SaveChangesAsync(ct);

        logger.LogInformation(
            "Vehicle photo {PhotoId} set as primary; listing {ListingId} reverted to Pending.",
            photo.Id, listing.Id);
    }
}
