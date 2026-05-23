using Bulletin.Board.Application.Storage;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Vehicles.Photos;

public sealed class DeleteVehiclePhotoCommandHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    IBlobStorage blobStorage,
    ICurrentUserService currentUser,
    ILogger<DeleteVehiclePhotoCommandHandler> logger)
    : IRequestHandler<DeleteVehiclePhotoCommand>
{
    public async Task Handle(DeleteVehiclePhotoCommand request, CancellationToken ct)
    {
        var photo = await vehicles.GetPhotoByIdAsync(request.PhotoId, ct)
            ?? throw new InvalidOperationException("Photo not found.");

        var listing = await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, photo.Vehicle.ListingId, ct);

        var filePath = photo.FilePath;

        vehicles.RemovePhoto(photo);
        listing.RequireReapproval();
        await vehicles.SaveChangesAsync(ct);

        // Best-effort delete of the underlying blob; DB row already gone, blob is just cleanup.
        await blobStorage.DeleteAsync(filePath, ct);

        logger.LogInformation(
            "Vehicle photo {PhotoId} deleted; listing {ListingId} reverted to Pending.",
            photo.Id, listing.Id);
    }
}
